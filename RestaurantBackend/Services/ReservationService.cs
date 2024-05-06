using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Http; 
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantBackend.Data;
using RestaurantBackend.Models;
using RestaurantBackend.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace RestaurantBackend.Services
{
    public interface IReservationService
    {
        Task<Reservation> CreateReservationAsync(Reservation reservation, string emailAddress);
        Task<bool> UpdateReservationAsync(Reservation reservation);
        Task<bool> CancelReservationAsync(int reservationId);
        Task<Reservation> GetReservationByIdAsync(int reservationId);
        Task<IEnumerable<Reservation>> GetAllReservationsAsync();
        Task SendConfirmationEmail(Customer customer, Reservation reservation); 
        // Task<string> GenerateTemporaryPassword(int length = 8);
    }

    public class ReservationService : IReservationService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Customer> _userManager;
        private readonly ILogger<ReservationService> _logger;
        private readonly IConfiguration _configuration;
        private readonly ICustomerService _customerService; 
        private readonly IPasswordService _passwordService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUrlHelperFactory _urlHelperFactory;

        public ReservationService(ApplicationDbContext context, UserManager<Customer> userManager, 
        ILogger<ReservationService> logger, IConfiguration configuration, ICustomerService customerService, 
        IPasswordService passwordService, IHttpContextAccessor httpContextAccessor, IUrlHelperFactory urlHelperFactory)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _configuration = configuration;
            _customerService = customerService;
            _passwordService = passwordService;
            _httpContextAccessor = httpContextAccessor;
            _urlHelperFactory = urlHelperFactory;
        }

        public async Task<Reservation> CreateReservationAsync(Reservation reservation, string emailAddress)
        {
            _logger.LogInformation("Attempting to find or create user for email: {Email}", emailAddress);
            var user = await _userManager.FindByEmailAsync(emailAddress);
            if (user == null)
            {
                _logger.LogInformation("No user found for email {Email}, creating new user.", emailAddress);
                user = new Customer { Email = emailAddress, UserName = emailAddress };
                var createUserResult = await _userManager.CreateAsync(user);
                if (!createUserResult.Succeeded)
                {
                    _logger.LogError("Failed to create a new user for the email: {Email}", emailAddress);
                    return null;
                }
            }

            _logger.LogInformation("User created or retrieved with email: {Email}", user.Email);

            reservation.UserId = user.Id;
            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Sending confirmation email to {Email}", emailAddress);
            await SendConfirmationEmail(user, reservation);

            return reservation;
        }

        public async Task<bool> UpdateReservationAsync(Reservation reservation)
        {
            var existingReservation = await _context.Reservations.FindAsync(reservation.ReservationId);
            if (existingReservation == null)
            {
                _logger.LogWarning("Reservation with ID {id} not found for update.", reservation.ReservationId);
                return false;
            }

            _context.Entry(existingReservation).CurrentValues.SetValues(reservation);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated reservation ID {id}", reservation.ReservationId);
            return true;
        }

        public async Task<bool> CancelReservationAsync(int reservationId)
        {
            var reservation = await _context.Reservations.FindAsync(reservationId);
            if (reservation == null)
            {
                _logger.LogWarning("Reservation with ID {id} not found for cancellation.", reservationId);
                return false;
            }

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Cancelled reservation ID {id}", reservationId);
            return true;
        }

        public async Task<Reservation> GetReservationByIdAsync(int reservationId)
        {
            return await _context.Reservations.FindAsync(reservationId);
        }

        public async Task<IEnumerable<Reservation>> GetAllReservationsAsync()
        {
            try
            {
                return await _context.Reservations.ToListAsync();
            }
            catch (Exception ex) // Ensure that ex is defined in a catch block
            {
                _logger.LogError(ex, "Failed to retrieve all reservations.");
                throw; // Rethrow the exception or handle it as needed
            }
        }

        public async Task SendConfirmationEmail(Customer customer, Reservation reservation)
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
            var passwordResetLink = $"{frontendBaseUrl}/reset-password?token={tokenEncoded}&email={emailEncoded}";

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
