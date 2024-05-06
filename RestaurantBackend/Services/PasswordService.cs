using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using RestaurantBackend.Data;
using RestaurantBackend.Models;
using RestaurantBackend.ViewModels;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantBackend.Services
{
    public interface IPasswordService
    {

        Task<IdentityResult> ChangePasswordAsync(Customer Customer, string currentPassword, string newPassword);
        Task<IdentityResult> ResetPasswordAsync(string userId, string token, string newPassword);
    }

    public class PasswordService : IPasswordService
    {
        private readonly UserManager<Customer> _userManager;
        private readonly ILogger<PasswordService> _logger;

        public PasswordService(UserManager<Customer> userManager, ILogger<PasswordService> logger)
        {
            _userManager = userManager;
            _logger = logger;
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

        public async Task<IdentityResult> ResetPasswordAsync(string userId, string token, string newPassword)
        {
            var customer = await _userManager.FindByIdAsync(userId);
            if (customer == null)
            {
                _logger.LogError($"User with ID {userId} not found.");
                return IdentityResult.Failed(new IdentityError { Description = "User not found1" });
            }

            // Attempt to reset the password using the provided token and new password
            var result = await _userManager.ResetPasswordAsync(customer, token, newPassword);
            if (!result.Succeeded)
            {
                // Log specific errors for each failure reason
                foreach (var error in result.Errors)
                {
                    _logger.LogError($"Password reset failed for user {customer.UserName}: {error.Description}");
                }
            }
            return result;
        }

    }

}
