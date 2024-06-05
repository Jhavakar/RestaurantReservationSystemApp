using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestaurantBackend.Data;
using RestaurantBackend.Models;
using RestaurantBackend.ViewModels;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace RestaurantBackend.Services
{
    public interface IPasswordService
    {

        Task<IdentityResult> ChangePasswordAsync(Customer Customer, string currentPassword, string newPassword);
        Task<IdentityResult> ResetPasswordAsync(string userId, string token, string newPassword);
        Task<IdentityResult> UpdatePasswordAsync(string userId, string currentPassword, string newPassword);
        Task SendResetPasswordEmailAsync(Customer customer, ForgotPasswordVM forgotPasswordVM);
    }

    public class PasswordService : IPasswordService
    {
        private readonly UserManager<Customer> _userManager;
        private readonly ILogger<PasswordService> _logger;
        private readonly IConfiguration _configuration;

        public PasswordService(UserManager<Customer> userManager, ILogger<PasswordService> logger, IConfiguration configuration)
        {
            _userManager = userManager;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<IdentityResult> ChangePasswordAsync(Customer customer, string currentPassword, string newPassword)
        {
            if (customer == null)
            {
                _logger.LogError("Cannot change password, user is null.");
                return IdentityResult.Failed(new IdentityError { Description = "User is null" });
            }

            if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword))
            {
                _logger.LogError("Cannot change password, current or new password is null or empty.");
                return IdentityResult.Failed(new IdentityError { Description = "Invalid password values" });
            }

            var result = await _userManager.ChangePasswordAsync(customer, currentPassword, newPassword);
            if (!result.Succeeded)
            {
                _logger.LogError($"Failed to change password for user {customer.UserName}");
            }
            return result;
        }

        public async Task<IdentityResult> ResetPasswordAsync(string email, string token, string newPassword)
        {
            var customer = await _userManager.FindByEmailAsync(email);
            if (customer == null)
            {
                _logger.LogError($"User with email {email} not found.");
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });
            }

            var result = await _userManager.ResetPasswordAsync(customer, token, newPassword);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    _logger.LogError($"Password reset failed for user {customer.UserName}: {error.Description}");
                }
            }
            return result;
        }

        public async Task<IdentityResult> UpdatePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            var customer = await _userManager.FindByIdAsync(userId);
            if (customer == null)
            {
                _logger.LogError("User not found.");
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });
            }

            var checkPassword = await _userManager.CheckPasswordAsync(customer, currentPassword);
            if (!checkPassword)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Invalid current password" });
            }

            return await _userManager.ChangePasswordAsync(customer, currentPassword, newPassword);
        }

        public async Task SendResetPasswordEmailAsync(Customer customer, ForgotPasswordVM forgotPasswordVM)
        {
            if (customer == null || string.IsNullOrEmpty(customer.Email))
            {
                _logger.LogError("Invalid customer data. Email is missing.");
                return;
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(customer);

            string frontendBaseUrl = _configuration["FrontendBaseUrl"];
            var tokenEncoded = WebUtility.UrlEncode(token);
            var emailEncoded = WebUtility.UrlEncode(customer.Email);
            var passwordResetLink = $"{frontendBaseUrl}/password-reset?token={tokenEncoded}&email={emailEncoded}";

            _logger.LogInformation($"Password reset link: {passwordResetLink}");

            try
            {
                using var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(_configuration["EmailSettings:EmailAddress"], _configuration["EmailSettings:Password"]),
                    EnableSsl = true,
                };

                using var message = new MailMessage(_configuration["EmailSettings:EmailAddress"], customer.Email)
                {
                    Subject = "Reset Your Password",
                    Body = $@"
                        <html>
                            <body>
                                <p>Hello,</p>
                                <p>Please click on the link below to reset your password, or copy the provided token and paste it on the password reset page:</p>
                                <p><a href='{passwordResetLink}'>Reset Password</a></p>
                                <p>If you prefer to enter the token manually, here is your reset token:</p>
                                <p><strong>Token:</strong> {token}</p>
                                <p>If you did not request a password reset, please ignore this email or contact support.</p>
                            </body>
                        </html>",
                    IsBodyHtml = true
                };

                await smtpClient.SendMailAsync(message);
                _logger.LogInformation("Sent password reset email to {Email}", customer.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to {Email}", customer.Email);
            }
        }

    }
}
