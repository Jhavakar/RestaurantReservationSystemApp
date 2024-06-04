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
        Task<Reservation?> CreateReservationAsync(ReservationVM model);
        Task<Reservation?> UpdateReservationAsync(ReservationVM model);
        Task<bool> CancelReservationAsync(int reservationId);
        Task<Reservation?> GetReservationByIdAsync(int reservationId);
        Task<IEnumerable<Reservation>> GetReservationsByEmailAsync(string email);
        Task<IEnumerable<Reservation>> GetAllReservationsAsync();
        Task SendConfirmationEmail(Customer customer, Reservation reservation);
        Task<IEnumerable<string>> GetAvailableSlotsAsync(DateTime date);

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

        public async Task<Reservation?> CreateReservationAsync(ReservationVM model)
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
                    PhoneNumber = model.PhoneNumber,
                };

                var result = await _userManager.CreateAsync(customer);
                if (!result.Succeeded)
                {
                    _logger.LogError("Failed to create a new user for the email: {Email}. Errors: {Errors}", model.EmailAddress, string.Join(", ", result.Errors.Select(e => e.Description)));
                    return null;
                }
            }
    
            try
            {
                // Log the input date and time for debugging purposes
                _logger.LogInformation("Received reservation date: {ReservationDate} and time: {ReservationTime} for creation", model.ReservationDate, model.ReservationTime);

                // Ensure the date is in the correct format before parsing
                if (!DateTime.TryParseExact(model.ReservationDate, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime reservationDate))
                {
                    _logger.LogError("Invalid reservation date format: {ReservationDate}", model.ReservationDate);
                    return null;
                }

                // Extract the time portion if it is in ISO 8601 format
                string reservationTimeStr = model.ReservationTime;
                if (model.ReservationTime.Contains("T"))
                {
                    try
                    {
                        reservationTimeStr = DateTime.Parse(model.ReservationTime).ToString("HH:mm");
                    }
                    catch (FormatException ex)
                    {
                        _logger.LogError("Failed to parse ISO 8601 time format: {ReservationTime}. Exception: {Exception}", model.ReservationTime, ex.Message);
                        return null;
                    }
                }

                // Ensure the time is in the correct format before parsing
                if (!DateTime.TryParseExact(reservationTimeStr, "HH:mm", null, System.Globalization.DateTimeStyles.None, out DateTime reservationTime))
                {
                    _logger.LogError("Invalid reservation time format: {ReservationTime}", reservationTimeStr);
                    return null;
                }

                // Combine the date and time into a single DateTime object
                DateTime reservationDateTime = reservationDate.Add(reservationTime.TimeOfDay);

                // Log the combined date and time
                _logger.LogInformation("Combined reservation DateTime: {ReservationDateTime}", reservationDateTime);

                // Validate the reservation time slot
                if (!IsValidTimeSlot(reservationDateTime))
                {
                    _logger.LogWarning("Invalid reservation time slot: {ReservationDateTime}", reservationDateTime);
                    return null;
                }

                // Create the reservation
                var reservation = new Reservation
                {
                    UserId = customer.Id,
                    ReservationDateTime = reservationDateTime,
                    NumberOfGuests = model.NumberOfGuests,
                    SpecialRequests = string.IsNullOrEmpty(model.SpecialRequests) ? "No special requests" : model.SpecialRequests,

                };

                // Validate the reservation details
                var validationContext = new ValidationContext(reservation);
                var validationResults = new List<ValidationResult>();
                bool isValid = Validator.TryValidateObject(reservation, validationContext, validationResults, true);
                if (!isValid)
                {
                    foreach (var validationResult in validationResults)
                    {
                        _logger.LogWarning("Validation error: {ErrorMessage}", validationResult.ErrorMessage);
                    }
                    return null;
                }

                // Check if the requested reservation slot is available
                bool isAvailable = await IsReservationSlotAvailableAsync(reservation.ReservationDateTime);
                if (!isAvailable)
                {
                    _logger.LogWarning("Requested reservation slot is unavailable: {ReservationDateTime}", reservation.ReservationDateTime);
                    return null;
                }

                try
                {
                    _context.Reservations.Add(reservation);
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while saving the reservation.");
                    return null;
                }

                _logger.LogInformation("User created or retrieved with email: {Email}", customer.Email);
                _logger.LogInformation("Reservation created successfully.");

                // Send confirmation email
                _logger.LogInformation("Sending confirmation email to {Email}", customer.Email);
                await SendConfirmationEmail(customer, reservation);

                return reservation;
            }
            catch (FormatException ex)
            {
                _logger.LogWarning("Invalid reservation date/time format: {ReservationDate} {ReservationTime}. Exception: {Exception}", model.ReservationDate, model.ReservationTime, ex.Message);
                return null;
            }
        }

        public async Task<Reservation?> UpdateReservationAsync(ReservationVM model)
    {
        _logger.LogInformation("Received reservation ID: {ReservationId}", model.ReservationId);

        if (model.ReservationId == 0)
        {
            _logger.LogError("Reservation ID is zero or invalid.");
            return null;
        }

        _logger.LogInformation("Received reservation date: {ReservationDate} and time: {ReservationTime} for update", model.ReservationDate, model.ReservationTime);

        if (string.IsNullOrWhiteSpace(model.ReservationDate) || string.IsNullOrWhiteSpace(model.ReservationTime))
        {
            _logger.LogWarning("Reservation date and time is missing. Date: {ReservationDate}, Time: {ReservationTime}", model.ReservationDate, model.ReservationTime);
            return null;
        }

        if (!DateTime.TryParseExact(model.ReservationDate, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime reservationDate))
        {
            _logger.LogWarning("Invalid reservation date format: {ReservationDate}", model.ReservationDate);
            return null;
        }

        string reservationTimeStr = model.ReservationTime;
        if (model.ReservationTime.Contains("T"))
        {
            try
            {
                reservationTimeStr = DateTime.Parse(model.ReservationTime).ToString("HH:mm");
            }
            catch (FormatException ex)
            {
                _logger.LogWarning("Failed to parse ISO 8601 time format: {ReservationTime}. Exception: {Exception}", model.ReservationTime, ex.Message);
                return null;
            }
        }

        if (!DateTime.TryParseExact(reservationTimeStr, "HH:mm", null, System.Globalization.DateTimeStyles.None, out DateTime reservationTime))
        {
            _logger.LogWarning("Invalid reservation time format: {ReservationTime}", reservationTimeStr);
            return null;
        }

        DateTime reservationDateTime = reservationDate.Add(reservationTime.TimeOfDay);

        _logger.LogInformation("Combined reservation DateTime: {ReservationDateTime}", reservationDateTime);

        if (!IsValidTimeSlot(reservationDateTime))
        {
            _logger.LogWarning("Invalid reservation time slot: {ReservationDateTime}", reservationDateTime);
            return null;
        }

        var existingReservation = await _context.Reservations
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.ReservationId == model.ReservationId);

        if (existingReservation == null)
        {
            _logger.LogWarning("Reservation with ID {ReservationId} not found for update.", model.ReservationId);
            return null;
        }

        var customer = existingReservation.User;
        if (customer == null)
        {
            _logger.LogWarning("Customer not found for reservation ID {ReservationId}.", model.ReservationId);
            return null;
        }

        _logger.LogInformation("Found customer with ID {CustomerId} for reservation ID {ReservationId}.", customer.Id, model.ReservationId);

        if (customer.FirstName != model.FirstName || customer.LastName != model.LastName || customer.Email != model.EmailAddress || customer.PhoneNumber != model.PhoneNumber)
        {
            customer.FirstName = model.FirstName;
            customer.LastName = model.LastName;
            customer.Email = model.EmailAddress;
            customer.PhoneNumber = model.PhoneNumber;

            await _userManager.UpdateAsync(customer);
        }

        existingReservation.ReservationDateTime = reservationDateTime;
        existingReservation.NumberOfGuests = model.NumberOfGuests;
        existingReservation.SpecialRequests = model.SpecialRequests;

        _context.Reservations.Update(existingReservation);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated reservation ID {ReservationId}", model.ReservationId);

        await SendUpdateEmail(customer, existingReservation);

        return existingReservation;
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

        public async Task<Reservation?> GetReservationByIdAsync(int reservationId)
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve all reservations.");
                throw; // Rethrow the exception or handle it as needed
            }
        }

        public async Task<bool> IsReservationSlotAvailableAsync(DateTime desiredReservationTime)
        {
            return !await _context.Reservations
                .AnyAsync(r => r.ReservationDateTime == desiredReservationTime);
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

        public async Task SendUpdateEmail(Customer customer, Reservation reservation)
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
                    Subject = "Update Confirmation",
                    Body = $@"
                        <html>
                            <body>
                                <p>Hello {customer.FirstName},</p>
                                <p>Your reservation has been updated successfully. Here are the updated details:</p>
                                <p><strong>Reservation DateTime:</strong> {reservation.ReservationDateTime}</p>
                                <p><strong>Number of Guests:</strong> {reservation.NumberOfGuests}</p>
                                <p>If you have any questions, please contact us.</p>
                            </body>
                        </html>",
                    IsBodyHtml = true
                };

                await smtpClient.SendMailAsync(message);
                _logger.LogInformation("Sent update confirmation email to {Email}", customer.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send update confirmation email to {Email}", customer.Email);
            }
        }

        public bool IsValidTimeSlot(DateTime reservationTime)
        {
            return reservationTime.Minute == 0 || reservationTime.Minute == 30;
        }

        public async Task<IEnumerable<string>> GetAvailableSlotsAsync(DateTime date)
    {
        return await _context.Reservations
            .Where(r => r.ReservationDateTime.Date == date.Date)
            .Select(r => r.ReservationDateTime.ToString("HH:mm"))
            .ToListAsync();
    }

    }
}
