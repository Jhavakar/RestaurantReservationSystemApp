using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RestaurantBackend.Models;
using RestaurantBackend.Services;
using RestaurantBackend.Utility;
using RestaurantBackend.ViewModels;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly UserManager<Customer> _userManager;
    private readonly SignInManager<Customer> _signInManager;
    private readonly AppSettings _appSettings;
    
    
    public UsersController(IConfiguration configuration, UserManager<Customer> userManager, 
        SignInManager<Customer> signInManager, IOptions<AppSettings> appSettings)
    {
        _configuration = configuration;
        _userManager = userManager;
        _signInManager = signInManager;
        _appSettings = appSettings.Value;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginVM loginVM)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _userManager.FindByEmailAsync(loginVM.EmailAddress);
        if (user == null)
        {
            return Unauthorized("User not found.");
        }

        // Check if the password is correct
        var result = await _signInManager.PasswordSignInAsync(user, loginVM.Password, isPersistent: false, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            return Unauthorized("Invalid login attempt.");
        }

        // Generate JWT token
        var token = GenerateJwtToken(user);
        return Ok(new { success = true, token });
    }

    // [HttpPost("logout")]
    // public IActionResult Logout()
    // {
    //     Response.Cookies.Delete("auth_token", new CookieOptions { HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict });
    //     return Ok(new { message = "Successfully logged out" });
    // }

    private string GenerateJwtToken(IdentityUser user)
    {
        var key = _appSettings.Secret ?? throw new InvalidOperationException("JWT Secret is not configured.");
        if (key.Length < 32)
        {
            throw new InvalidOperationException("JWT Secret must be at least 32 characters long.");
        }

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserName ?? "unknown_user"),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? "no-email@example.com"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var tokenDescriptor = new JwtSecurityToken(
            issuer: _appSettings.Issuer,
            audience: _appSettings.Audience,
            claims: claims,
            expires: DateTime.Now.AddHours(24),
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
