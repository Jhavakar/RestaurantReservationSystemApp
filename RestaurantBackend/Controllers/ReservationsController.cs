using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RestaurantBackend.Models;
using RestaurantBackend.Services;
using RestaurantBackend.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReservationsController : ControllerBase
    {
        private readonly IReservationService _reservationService;
        private readonly ILogger<ReservationsController> _logger;

        public ReservationsController(IReservationService reservationService, ILogger<ReservationsController> logger)
        {
            _reservationService = reservationService;          
            _logger = logger;
        }

        // POST: api/Reservations
        [HttpPost]
        public async Task<IActionResult> CreateReservation([FromBody] ReservationVM model)
        {
            try
            {
                var reservation = await _reservationService.CreateReservationWithCustomerAsync(model);
                return CreatedAtAction(nameof(GetReservationById), new { id = reservation.ReservationId }, reservation);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error creating reservation.");
                return BadRequest("Error creating reservation.");
            }
        }

        // GET: api/Reservations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Reservation>>> GetReservations()
        {
            return Ok(await _reservationService.GetAllReservationsAsync());
        }

        // GET: api/Reservations/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReservationById(int id)
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            if (reservation == null)
            {
                _logger.LogWarning("Reservation with ID {Id} not found.", id);
                return NotFound();
            }
            return Ok(reservation);
        }

        // PUT: api/Reservations/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReservation(int id, [FromBody] Reservation reservation)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != reservation.ReservationId)
            {
                return BadRequest("Mismatched reservation ID.");
            }

            try
            {
                await _reservationService.UpdateReservationAsync(reservation);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Reservation with ID {id} not found.");
            }
            catch (System.Exception ex)
            {
                _logger.LogError("Error updating reservation: {ExceptionMessage}", ex.Message);
                return BadRequest("Error updating reservation.");
            }
        }

        // DELETE: api/Reservations/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReservation(int id)
        {
            try
            {
                await _reservationService.CancelReservationAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Reservation with ID {id} not found.");
            }
        }
    }
}
