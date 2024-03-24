using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantBackend.Data;
using RestaurantBackend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantBackend.Services {

    public interface ITableService
    {
        Task<IEnumerable<Table>> GetAllTablesAsync();
        Task<Table> GetTableByIdAsync(int tableId);
        Task<IEnumerable<Table>> GetAvailableTablesAsync(DateTime date, TimeSpan duration, int numberOfGuests);
        Task<Reservation> ReserveTableAsync(int customerId, DateTime reservationTime, TimeSpan duration, int numberOfGuests);
    }

    public class TableService : ITableService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TableService> _logger;

        public TableService(ApplicationDbContext context, ILogger<TableService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Table>> GetAllTablesAsync()
        {
            return await _context.Tables.ToListAsync();
        }

        public async Task<Table> GetTableByIdAsync(int tableId)
        {
            return await _context.Tables.FindAsync(tableId);
        }

        public async Task<IEnumerable<Table>> GetAvailableTablesAsync(DateTime date, TimeSpan duration, int numberOfGuests)
        {
            var endDateTime = date.Add(duration);
            var reservedTableIds = await _context.Reservations
                .Where(r => r.ReservationTime >= date && r.ReservationEndTime <= endDateTime)
                .Select(r => r.TableId)
                .Distinct()
                .ToListAsync();

            return await _context.Tables
                .Where(t => !reservedTableIds.Contains(t.TableId) && t.Capacity >= numberOfGuests)
                .ToListAsync();
        }

        public async Task<Reservation> ReserveTableAsync(int customerId, DateTime reservationTime, TimeSpan duration, int numberOfGuests)
        {
            var availableTables = await GetAvailableTablesAsync(reservationTime, duration, numberOfGuests);
            var table = availableTables.FirstOrDefault();
            if (table == null)
            {
                _logger.LogWarning("No available tables for the specified time and guest count.");
                return null;
            }

            var reservation = new Reservation
            {
                CustomerId = customerId,
                TableId = table.TableId,
                ReservationTime = reservationTime,
                ReservationEndTime = reservationTime.Add(duration),
                // Assuming you have fields like NumberOfGuests in your Reservation model
                NumberOfGuests = numberOfGuests,
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            return reservation;
        }
    }
}
