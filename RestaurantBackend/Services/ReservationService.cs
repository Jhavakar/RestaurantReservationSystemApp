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
using System.Threading.Tasks;

namespace RestaurantBackend.Services
{
    public interface IReservationService
    {
        Task<Reservation> CreateReservationAsync(Reservation reservation);
        Task<Reservation> CreateReservationWithCustomerAsync(ReservationVM model);
        Task<Reservation> GetReservationByIdAsync(int reservationId);
        Task<IEnumerable<Reservation>> GetAllReservationsAsync();
        Task UpdateReservationAsync(Reservation reservation);
        Task CancelReservationAsync(int reservationId);
    }

    public class ReservationService : IReservationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReservationService> _logger;
        private readonly IConfiguration _configuration;
        private readonly ICustomerService _customerService; 

        public ReservationService(ApplicationDbContext context, ILogger<ReservationService> logger, IConfiguration configuration, ICustomerService customerService)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
            _customerService = customerService;
        }

        public async Task<Reservation> CreateReservationAsync(Reservation reservation)
        {
            // Correctly setting ReservationEndTime to 1 hour after ReservationTime
            reservation.ReservationEndTime = reservation.ReservationTime.AddHours(1);

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Reservation created successfully.");
            return reservation; // This is correct; no need to redeclare a new Reservation object.
        }

        public async Task<Reservation> CreateReservationWithCustomerAsync(ReservationVM model)
        {
            // Attempt to find an existing customer by email address using the CustomerService
            var customer = await _customerService.GetCustomerByEmailAsync(model.EmailAddress);

            // If the customer does not exist, create a new one
            if (customer == null)
            {
                customer = new Customer
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailAddress = model.EmailAddress.Trim(),
                };

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync(); // This line generates CustomerId
            }

            // Your existing logic to create a reservation...
            var reservation = new Reservation
            {
                CustomerId = customer.CustomerId,
                ReservationTime = model.ReservationTime,
                NumberOfGuests = model.NumberOfGuests,
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync(); // This line generates ReservationId

            // Correctly call SendConfirmationEmail with both required parameters
            await SendConfirmationEmail(reservation, customer.EmailAddress);

            return reservation; 
        }

        public async Task<Reservation> GetReservationByIdAsync(int reservationId)
        {
            return await _context.Reservations.FindAsync(reservationId);
        }

        public async Task<IEnumerable<Reservation>> GetAllReservationsAsync()
        {
            return await _context.Reservations.ToListAsync();
        }

        public async Task UpdateReservationAsync(Reservation reservation)
        {
            try
            {
                var existingReservation = await _context.Reservations.FindAsync(reservation.ReservationId);
                if (existingReservation == null)
                {
                    _logger.LogWarning($"Reservation with ID {reservation.ReservationId} not found for update.");
                    throw new KeyNotFoundException($"Reservation with ID {reservation.ReservationId} not found.");
                }                

                _context.Entry(existingReservation).CurrentValues.SetValues(reservation);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Reservation updated: {reservation}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while updating reservation with ID {reservation.ReservationId}.");
                throw; // Re-throwing the exception preserves the stack trace but allows for custom logging
            }
        }

        public async Task CancelReservationAsync(int reservationId)
        {
            var reservation = await _context.Reservations.FindAsync(reservationId);
            if (reservation != null)
            {
                _context.Reservations.Remove(reservation);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Reservation with ID {ReservationId} deleted.", reservationId);
            }
            else
            {
                _logger.LogWarning("Reservation with ID {ReservationId} not found for deletion.", reservationId);
        }
        }

        // private async Task<bool> IsTableAvailableAsync(int tableId, DateTime requestedTime, TimeSpan duration)
        // {
        //     var overlappingReservations = await _context.Reservations
        //         .Where(r => r.TableId == tableId &&
        //                     ((r.ReservationTime <= requestedTime && r.ReservationEndTime > requestedTime) ||
        //                      (requestedTime.Add(duration) > r.ReservationTime && requestedTime.Add(duration) <= r.ReservationEndTime)))
        //         .ToListAsync();

        //     return !overlappingReservations.Any();
        //     }

        // private async Task<bool> IsTableAvailableAsync(int tableId, DateTime requestedTime, TimeSpan duration)
        // {
        //     var overlappingReservations = await _context.Reservations
        //         .Where(r => r.TableId == tableId &&
        //                     ((r.ReservationTime <= requestedTime && r.ReservationEndTime > requestedTime) ||
        //                      (requestedTime.Add(duration) > r.ReservationTime && requestedTime.Add(duration) <= r.ReservationEndTime)))
        //         .ToListAsync();

        //     return !overlappingReservations.Any();
        // }
    
        private async Task SendConfirmationEmail(Reservation reservation, string customerEmail)
        {
            try
            {
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(_configuration["EmailSettings:EmailAddress"], _configuration["EmailSettings:Password"]),
                    EnableSsl = true,
                };

                using (var message = new MailMessage(_configuration["EmailSettings:EmailAddress"], customerEmail))
                {
                    message.Subject = "Your reservation confirmation";
                    // Removed reference to customer.FirstName
                    message.Body = $"Your reservation for {reservation.ReservationTime} with {reservation.NumberOfGuests} guests has been successfully made.";
                    await smtpClient.SendMailAsync(message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send confirmation email for reservation ID: {ReservationId}", reservation.ReservationId);
            }
        }

    }
}
