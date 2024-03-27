using Microsoft.AspNetCore.Mvc;
using RestaurantBackend.Models;
using RestaurantBackend.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class TableController : ControllerBase
{
    private readonly ITableService _tableService;

    public TableController(ITableService tableService)
    {
        _tableService = tableService;
    }

    // GET: api/Table
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Table>>> GetAllTables()
    {
        var tables = await _tableService.GetAllTablesAsync();
        return Ok(tables);
    }

    // GET: api/Table/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Table>> GetTableById(int id)
    {
        var table = await _tableService.GetTableByIdAsync(id);
        if (table == null)
        {
            return NotFound();
        }
        return Ok(table);
    }

    // GET: api/Table/available
    [HttpGet("available")]
    public async Task<IActionResult> GetAvailableTables([FromQuery] DateTime date, [FromQuery] TimeSpan duration, [FromQuery] int numberOfGuests)
    {
        var availableTables = await _tableService.GetAvailableTablesAsync(date, duration, numberOfGuests);
        return Ok(availableTables);
    }

    // POST: api/Table/reserve/{tableId}
    [HttpPost("reserve/{tableId}")]
    public async Task<IActionResult> ReserveTable(int tableId, [FromBody] Reservation reservationDetails)
    {
        try
        {
            // Assuming reservationDetails includes all necessary info
            // and you define a fixed duration for your reservations, e.g., 1 hour.
            TimeSpan duration = TimeSpan.FromHours(1); // Example fixed duration

            var reservation = await _tableService.ReserveTableAsync(
                reservationDetails.CustomerId, 
                reservationDetails.ReservationTime, 
                duration, 
                reservationDetails.NumberOfGuests);

            if (reservation != null)
            {
                return CreatedAtAction(nameof(GetTableById), new { id = tableId }, reservation);
            }
            else
            {
                return BadRequest("Failed to reserve table. It may no longer be available.");
            }
        }
        catch (Exception ex)
        {
            // Consider logging the exception details
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

}
