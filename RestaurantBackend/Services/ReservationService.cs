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
        Task<string> GenerateTemporaryPassword(int length = 8);
    }

    public class ReservationService : IReservationService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Customer> _userManager;
        private readonly ILogger<ReservationService> _logger;
        private readonly IConfiguration _configuration;
        private readonly ICustomerService _customerService; 

        public ReservationService(ApplicationDbContext context, UserManager<Customer> userManager, ILogger<ReservationService> logger, IConfiguration configuration, ICustomerService customerService)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _configuration = configuration;
            _customerService = customerService;
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

            // Additional user update logic if necessary
            _logger.LogInformation("User created or retrieved with email: {Email}", user.Email);

            // Set reservation details
            reservation.UserId = user.Id;
            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();


            // Send confirmation email with the reservation details and temporary password
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
                    Subject = "Your reservation confirmation",
                    Body = $"Dear {customer.FirstName},\n\n" +
                        $"Your reservation for {reservation.ReservationTime} with {reservation.NumberOfGuests} guests has been successfully made.\n\n" +
                        $"Your temporary password to manage your reservation is: {customer.TemporaryPassword}\n\n" +
                        $"Please use this password to log in and manage your reservation details. Note: This password will expire in 24 hours.\n\n" +
                        $"Best regards,\n" +
                        $"Your Restaurant Team",
                    IsBodyHtml = true
                };

                _logger.LogInformation("Sending confirmation email to {Email} with temporary password {TemporaryPassword}", customer.Email, customer.TemporaryPassword);
                await smtpClient.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send confirmation email to {Email}.", customer.Email);
            }
        }

        public async Task<string> GenerateTemporaryPassword(int length = 8)
        {
            const string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            using var rng = RandomNumberGenerator.Create(); // Correct usage of RandomNumberGenerator
            var bytes = new byte[length];
            rng.GetBytes(bytes);
            var chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                chars[i] = validChars[bytes[i] % validChars.Length];
            }
            return new string(chars);
        }
    
    }
}
