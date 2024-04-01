using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantBackend.Data;
using RestaurantBackend.Models;
using RestaurantBackend.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public ReservationService(ApplicationDbContext context, ILogger<ReservationService> logger)
        {
            _context = context;
            _logger = logger;
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
            // Attempt to find an existing customer by email address
            var customer = await _context.Customers
                                .FirstOrDefaultAsync(c => c.EmailAddress == model.EmailAddress);

            // If the customer does not exist, create a new one
            if (customer == null)
            {
                customer = new Customer
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailAddress = model.EmailAddress,
                };

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync(); // Save the new customer to generate a CustomerId
            }

            // Create the reservation linked to the identified or newly created customer
            var reservation = new Reservation
            {
                CustomerId = customer.CustomerId, 
                ReservationTime = model.ReservationTime,
                NumberOfGuests = model.NumberOfGuests,
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync(); // Save the new reservation

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
    
    }
}
