using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantBackend.Data;
using RestaurantBackend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantBackend.Services
{
    public interface IReservationService
    {
        Task<Reservation> CreateReservationAsync(Reservation reservation);
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
            var existingReservation = await _context.Reservations.FindAsync(reservation.ReservationId);
            if (existingReservation != null)
            {
                _context.Entry(existingReservation).CurrentValues.SetValues(reservation);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Reservation updated: {Id}", reservation.ReservationId);
            }
            else
            {
                throw new KeyNotFoundException($"Reservation with ID {reservation.ReservationId} not found.");
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
