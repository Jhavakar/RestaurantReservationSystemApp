using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RestaurantBackend.Models;
using RestaurantBackend.Services;
using RestaurantBackend.ViewModels;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly UserManager<Customer> _userManager;
    private readonly SignInManager<Customer> _signInManager;
    
    
    public UsersController(IConfiguration configuration, UserManager<Customer> userManager, SignInManager<Customer> signInManager)
    {
        _configuration = configuration;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    // [HttpPost("login")]
    // public async Task<IActionResult> Login([FromBody] LoginVM loginVM)
    // {
    //     var user = await _userManager.FindByEmailAsync(loginVM.EmailAddress);
    //     if (user != null && await _userManager.CheckPasswordAsync(user, loginVM.Password))
    //     {
    //         var token = GenerateJwtToken(user);  // Generate the JWT token for the user
    //         var cookieOptions = new CookieOptions
    //         {
    //             HttpOnly = true,
    //             Expires = DateTime.Now.AddHours(24),
    //             Secure = true,
    //             SameSite = SameSiteMode.Strict
    //         };
    //         Response.Cookies.Append("auth_token", token, cookieOptions);
    //         return Ok(new { message = "Login successful" });
    //     }
    //     return Unauthorized("Invalid login attempt.");
    // }

    // [HttpPost("logout")]
    // public IActionResult Logout()
    // {
    //     Response.Cookies.Delete("auth_token", new CookieOptions { HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict });
    //     return Ok(new { message = "Successfully logged out" });
    // }

    private string GenerateJwtToken(IdentityUser user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var tokenDescriptor = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(3),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }

    [HttpPost("ResetPassword")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordVM model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        // Validate the token and reset the password
        var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
        if (!result.Succeeded)
        {
            // Collect errors to return
            var errors = result.Errors.Select(e => e.Description);
            return BadRequest(new { Errors = errors });
        }

        if (result.Succeeded)
        {
            return Ok(new { success = true, message = "Password has been successfully reset." });
        }
        else
        {
            return Ok(new { success = false, message = "Password reset failed.", errors = result.Errors });
        }
    }

}
