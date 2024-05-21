using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RestaurantBackend.Data;
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
    private readonly ApplicationDbContext _context;

    public UsersController(IConfiguration configuration, UserManager<Customer> userManager, 
        SignInManager<Customer> signInManager, IOptions<AppSettings> appSettings, ApplicationDbContext context)
    {
        _configuration = configuration;
        _userManager = userManager;
        _signInManager = signInManager;
        _appSettings = appSettings.Value;
        _context = context;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginVM loginVM)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "Invalid input data." });
        }

        var user = await _userManager.FindByEmailAsync(loginVM.EmailAddress);
        if (user == null)
        {
            return Unauthorized(new { message = "User not found." });
        }

        // Verify user credentials
        var result = await _signInManager.PasswordSignInAsync(user, loginVM.Password, isPersistent: false, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            return Unauthorized(new { message = "Invalid login attempt. Please check your credentials." });
        }

        // Generate JWT token
        var token = GenerateJwtToken(user);
        return Ok(new { success = true, token });
    }

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

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordVM model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "Invalid input data.", model });
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return NotFound(new { message = "User with the specified email not found." });
        }

        // Validate the token and reset the password
        var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
        if (!result.Succeeded)
        {
            // Collect errors to return
            var errors = result.Errors.Select(e => e.Description);
            return BadRequest(new { message = "Password reset failed. Please try again.", errors });
        }

        return Ok(new { success = true, message = "Password has been successfully reset." });
    }

    [HttpGet("verify-and-fetch-reservation")]
    public async Task<IActionResult> VerifyAndFetchReservation(string email, string token)
    {
        var user = await _userManager.FindByEmailAsync(email) as Customer;
        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        var reservation = await _context.Reservations
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.UserId == user.Id);

        if (reservation == null)
        {
            return NotFound(new { message = "Reservation not found for the user." });
        }

        var response = new
        {
            FirstName = reservation.User.FirstName ?? "N/A",
            LastName = reservation.User.LastName ?? "N/A",
            Email = reservation.User.Email ?? "N/A",
            ReservationDetails = new
            {
                reservation.ReservationTime,
                reservation.NumberOfGuests
            },
            HasPassword = await _userManager.HasPasswordAsync(reservation.User)
        };

        // Log the response in the console for verification
        Console.WriteLine($"API Response: FirstName = {response.FirstName}, LastName = {response.LastName}, Email = {response.Email}");

        return Ok(response);
    }

    // Display the set password form
    [HttpGet("user/set-password")]
    public IActionResult SetPassword(string email)
    {
        var model = new UpdatePasswordVM { Email = email };
        return Ok(new { message = "Set password form initialized.", model });
    }

    // Process the set password form submission
    [HttpPost("user/set-password")]
    public async Task<IActionResult> SetPassword(UpdatePasswordVM model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "Invalid input data.", model });
        }

        // Find the user by email
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        // Check if the user already has a password
        var hasPassword = await _userManager.HasPasswordAsync(user);
        if (hasPassword)
        {
            return Conflict(new { message = "User already has a password. Please log in instead." });
        }

        // Add the password to the user's account
        var result = await _userManager.AddPasswordAsync(user, model.NewPassword);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            return BadRequest(new { message = "Password setting failed.", errors });
        }

        // Provide success response or redirect
        return Ok(new { success = true, message = "Password has been successfully set. You can now log in." });
    }
}
