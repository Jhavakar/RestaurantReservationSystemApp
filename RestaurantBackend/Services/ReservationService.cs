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
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Reservation> CreateReservationAsync(Reservation reservation)
        {
            // Check if the table is available at the requested time
            bool isAvailable = await IsTableAvailableAsync(reservation.TableId, reservation.ReservationTime, TimeSpan.FromHours(1));
            if (!isAvailable)
            {
                throw new InvalidOperationException("The table is not available for the selected time.");
            }

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();
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
            _context.Entry(reservation).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task CancelReservationAsync(int reservationId)
        {
            var reservation = await _context.Reservations.FindAsync(reservationId);
            if (reservation != null)
            {
                _context.Reservations.Remove(reservation);
                await _context.SaveChangesAsync();
            }
        }

        private async Task<bool> IsTableAvailableAsync(int tableId, DateTime requestedTime, TimeSpan duration)
        {
            var overlappingReservations = await _context.Reservations
                .Where(r => r.TableId == tableId &&
                            ((r.ReservationTime <= requestedTime && r.ReservationEndTime > requestedTime) ||
                             (requestedTime.Add(duration) > r.ReservationTime && requestedTime.Add(duration) <= r.ReservationEndTime)))
                .ToListAsync();

            return !overlappingReservations.Any();
        }
    }
}
