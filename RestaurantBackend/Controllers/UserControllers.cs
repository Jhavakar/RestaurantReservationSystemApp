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
using System.Linq;
using System.Security.Claims;
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
    private readonly IPasswordService _passwordService;

    public UsersController(IConfiguration configuration, UserManager<Customer> userManager, 
        SignInManager<Customer> signInManager, IOptions<AppSettings> appSettings, ApplicationDbContext context,
        IPasswordService passwordService)
    {
        _configuration = configuration;
        _userManager = userManager;
        _signInManager = signInManager;
        _appSettings = appSettings.Value;
        _context = context;
        _passwordService = passwordService;
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

        var result = await _signInManager.PasswordSignInAsync(user, loginVM.Password, isPersistent: false, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            return Unauthorized(new { message = "Invalid login attempt. Please check your credentials." });
        }

        var token = GenerateJwtToken(user);
        return Ok(new { success = true, token });
    }

    private string GenerateJwtToken(Customer user)
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
            PhoneNumber = reservation.User?.PhoneNumber ?? "N/A",
            ReservationDetails = new
            {
                ReservationDateTime = reservation.ReservationDateTime.ToString("o"),
                reservation.NumberOfGuests,
                reservation.SpecialRequests,
            },
            HasPassword = await _userManager.HasPasswordAsync(reservation.User)
        };

        Console.WriteLine($"API Response: FirstName = {response.FirstName}, LastName = {response.LastName}, Email = {response.Email}");

        return Ok(response);
    }

    [HttpGet("user/set-password")]
    public IActionResult SetPassword(string email)
    {
        var model = new UpdatePasswordVM { Email = email };
        return Ok(new { message = "Set password form initialized.", model });
    }

    [HttpPost("user/set-password")]
    public async Task<IActionResult> SetPassword(UpdatePasswordVM model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "Invalid input data.", model });
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        var hasPassword = await _userManager.HasPasswordAsync(user);
        if (hasPassword)
        {
            return Conflict(new { message = "User already has a password. Please log in instead." });
        }

        var result = await _userManager.AddPasswordAsync(user, model.NewPassword);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            return BadRequest(new { message = "Password setting failed.", errors });
        }

        if (!user.EmailConfirmed)
        {
            var confirmEmailResult = await _userManager.ConfirmEmailAsync(user, model.Token);
            if (!confirmEmailResult.Succeeded)
            {
                return BadRequest(new { message = "Email confirmation failed." });
            }
        }

        return Ok(new { success = true, message = "Password has been successfully set and email confirmed. You can now log in." });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordVM model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "Invalid input data." });
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return BadRequest(new { message = "User not found." });
        }

        await _passwordService.SendResetPasswordEmailAsync(user, model);

        return Ok(new { success = true, message = "Password reset link has been sent to your email." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordVM model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "Invalid input data." });
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            return BadRequest(new { message = "Password reset failed. Please try again.", errors });
        }

        return Ok(new { success = true, message = "Password has been successfully reset." });
    }

    [HttpPost("updatePassword/{userId}")]
    public async Task<IActionResult> UpdatePassword(string userId, [FromBody] UpdatePasswordVM model)
    {
        if (string.IsNullOrWhiteSpace(model.CurrentPassword) || string.IsNullOrWhiteSpace(model.NewPassword))
        {
            return BadRequest("Both current and new passwords are required.");
        }

        var result = await _passwordService.UpdatePasswordAsync(userId, model.CurrentPassword, model.NewPassword);
        if (result.Succeeded)
        {
            return Ok("Password updated successfully.");
        }
        else
        {
            return BadRequest(result.Errors.Select(e => e.Description));
        }
    }

}
