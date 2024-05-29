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
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace RestaurantBackend.Services
{
    public interface IReservationService
    {
        Task<Reservation> CreateReservationAsync(ReservationVM model);
        Task<bool> UpdateReservationAsync(Reservation reservation);
        Task<bool> CancelReservationAsync(int reservationId);
        Task<Reservation> GetReservationByIdAsync(int reservationId);
        Task<IEnumerable<Reservation>> GetReservationsByEmailAsync(string email);
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

        public async Task<Reservation> CreateReservationAsync(ReservationVM model)
        {
            _logger.LogInformation("Checking user existence by email: {Email}", model.EmailAddress);
            var customer = await _userManager.FindByEmailAsync(model.EmailAddress);

            if (customer == null)
            {
                customer = new Customer
                {
                    UserName = model.EmailAddress, 
                    Email = model.EmailAddress,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber // Set the phone number, which might be null
                };

                var result = await _userManager.CreateAsync(customer);
                if (!result.Succeeded)
                {
                    _logger.LogError("Failed to create a new user for the email: {Email}", model.EmailAddress);
                    return null;
                }
            }
            else
            {
                if (customer.FirstName != model.FirstName || customer.LastName != model.LastName || customer.PhoneNumber != model.PhoneNumber)
                {
                    _logger.LogWarning("Name mismatch or phone number mismatch for email {Email}. Existing records: {ExistingFirstName} {ExistingLastName} {ExistingPhoneNumber}, Provided: {ProvidedFirstName} {ProvidedLastName} {ProvidedPhoneNumber}",
                        model.EmailAddress, customer.FirstName, customer.LastName, customer.PhoneNumber, model.FirstName, model.LastName, model.PhoneNumber);

                    customer.FirstName = model.FirstName;
                    customer.LastName = model.LastName;
                    customer.PhoneNumber = model.PhoneNumber; // Update phone number
                    await _userManager.UpdateAsync(customer);
                }
            }

            // Create the reservation
            var reservation = new Reservation
            {
                UserId = customer.Id,
                ReservationTime = model.ReservationTime,
                NumberOfGuests = model.NumberOfGuests
            };

            // Validate the reservation details
            var validationContext = new ValidationContext(reservation);
            var validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(reservation, validationContext, validationResults, true);
            if (!isValid)
            {
                foreach (var validationResult in validationResults)
                {
                    _logger.LogWarning($"Validation error: {validationResult.ErrorMessage}");
                }
                return null;
            }

            // Check if the requested reservation slot is available
            bool isAvailable = await IsReservationSlotAvailableAsync(reservation.ReservationTime);
            if (!isAvailable)
            {
                _logger.LogWarning("Requested reservation slot is unavailable: {ReservationTime}", reservation.ReservationTime);
                return null;
            }

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User created or retrieved with email: {Email}", customer.Email);
            _logger.LogInformation("Reservation created successfully.");

            // Send confirmation email
            _logger.LogInformation("Sending confirmation email to {Email}", customer.Email);
            await SendConfirmationEmail(customer, reservation);

            return reservation;
        }

        public async Task<bool> UpdateReservationAsync(Reservation reservation)
        {
            var existingReservation = await _context.Reservations
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.ReservationId == reservation.ReservationId);
                
            if (existingReservation == null)
            {
                _logger.LogWarning("Reservation with ID {id} not found for update.", reservation.ReservationId);
                return false;
            }

            // Update user details if they have changed
            var customer = existingReservation.User;
            if (customer == null)
            {
                _logger.LogWarning("Customer not found for reservation ID {id}.", reservation.ReservationId);
                return false;
            }

            if (customer.FirstName != reservation.User.FirstName || customer.LastName != reservation.User.LastName || customer.Email != reservation.User.Email || customer.PhoneNumber != reservation.User.PhoneNumber)
            {
                customer.FirstName = reservation.User.FirstName;
                customer.LastName = reservation.User.LastName;
                customer.Email = reservation.User.Email;
                customer.PhoneNumber = reservation.User.PhoneNumber;

                await _userManager.UpdateAsync(customer);
            }

            // Update reservation details
            existingReservation.ReservationTime = reservation.ReservationTime;
            existingReservation.NumberOfGuests = reservation.NumberOfGuests;

            _context.Reservations.Update(existingReservation);
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
            var reservation = await _context.Reservations
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.ReservationId == reservationId);

            return reservation; // Return the entire reservation object or shape into a view model
        }
       
        public async Task<IEnumerable<Reservation>> GetReservationsByEmailAsync(string email)
        {
            // Find the customer first, and then fetch all reservations linked to that customer
            var customer = await _userManager.FindByEmailAsync(email);
            if (customer == null)
            {
                return Enumerable.Empty<Reservation>();
            }

            // Fetch all reservations by user/customer ID
            return await _context.Reservations
                .Where(r => r.UserId == customer.Id)
                .ToListAsync();
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

        public async Task<bool> IsReservationSlotAvailableAsync(DateTime desiredReservationTime)
        {
            // Check if any existing reservation has the same time
            bool isAvailable = !await _context.Reservations
                .AnyAsync(r => r.ReservationTime == desiredReservationTime);

            return isAvailable;
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
            var passwordResetLink = $"{frontendBaseUrl}/manage-reservation?token={tokenEncoded}&email={emailEncoded}";

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
