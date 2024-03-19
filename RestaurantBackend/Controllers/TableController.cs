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

    // GET: api/Table/availability
    [HttpGet("available")]
    public async Task<IActionResult> GetAvailableTables([FromQuery] DateTime date, [FromQuery] TimeSpan duration, [FromQuery] int numberOfGuests)
    {
        var availableTables = await _tableService.GetAvailableTablesAsync(date, duration, numberOfGuests);
        return Ok(availableTables);
    }

    // POST: api/Table/reserve
    // Add more details to this endpoint as needed, such as reservation time.
    [HttpPost("reserve/{tableId}")]
    public async Task<IActionResult> ReserveTable(int tableId, [FromBody] int reservationId)
    {
        var success = await _tableService.ReserveTableAsync(tableId, reservationId);
        if (!success)
        {
            return BadRequest("Failed to reserve table.");
        }
        return Ok();
    }
    

    // Additional endpoints as needed...

}
