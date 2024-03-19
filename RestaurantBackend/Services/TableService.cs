using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestaurantBackend.Models;
using RestaurantBackend.Data;

namespace RestaurantBackend.Services {
    
    public interface ITableService
    {
        Task<IEnumerable<Table>> GetAllTablesAsync();
        Task<Table> GetTableByIdAsync(int tableId);
        // Updated to include the number of guests
        Task<IEnumerable<Table>> GetAvailableTablesAsync(DateTime date, TimeSpan duration, int numberOfGuests);
        Task<bool> ReserveTableAsync(int tableId, int reservationId);
    }

    public class TableService : ITableService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TableService> _logger;

        public TableService(ApplicationDbContext context, ILogger<TableService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<Table>> GetAllTablesAsync()
        {
            return await _context.Tables.ToListAsync();
        }

        public async Task<Table> GetTableByIdAsync(int tableId)
        {
            var table = await _context.Tables.FindAsync(tableId);
            if (table == null)
            {
                _logger.LogWarning($"Table {tableId} not found");
                throw new KeyNotFoundException($"Table {tableId} not found");
            }
            return table;
        }

        // Updated method to consider the number of guests
        public async Task<IEnumerable<Table>> GetAvailableTablesAsync(DateTime date, TimeSpan duration, int numberOfGuests)
        {
            var endDateTime = date.Add(duration);
            var reservedTableIds = await _context.Reservations
                .Where(r => r.ReservationTime >= date && r.ReservationTime < endDateTime)
                .Select(r => r.TableId)
                .Distinct()
                .ToListAsync();

            var availableTables = await _context.Tables
                .Where(t => !reservedTableIds.Contains(t.TableId) && t.Capacity >= numberOfGuests)
                .ToListAsync();

            

            return availableTables;
        }

        public async Task<bool> ReserveTableAsync(int tableId, int reservationId)
        {
            var table = await _context.Tables.FindAsync(tableId);
            var reservation = await _context.Reservations.FindAsync(reservationId);

            if (table != null && reservation != null)
            {
                reservation.TableId = tableId;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
}
