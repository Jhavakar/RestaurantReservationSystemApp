using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RestaurantBackend.Models;
using RestaurantBackend.Services;
using RestaurantBackend.ViewModels;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RestaurantBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize]
    public class ReservationsController : ControllerBase
    {
        private readonly IReservationService _reservationService;
        private readonly ILogger<ReservationsController> _logger;
        private readonly IMapper _mapper;

        public ReservationsController(IReservationService reservationService, ILogger<ReservationsController> logger, IMapper mapper)
        {
            _reservationService = reservationService;
            _logger = logger;
            _mapper = mapper;
        }

        // POST: api/Reservations
        [HttpPost]
        public async Task<IActionResult> CreateReservation([FromBody] ReservationVM model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdReservation = await _reservationService.CreateReservationAsync(model);
                if (createdReservation == null)
                {
                    _logger.LogError("Failed to create reservation.");
                    return BadRequest("Failed to create the reservation.");
                }

                var responseModel = _mapper.Map<ReservationVM>(createdReservation);
                return CreatedAtAction(nameof(GetReservationById), new { id = createdReservation.ReservationId }, responseModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating reservation.");
                return StatusCode(500, "An error occurred while creating the reservation.");
            }
        }

        // GET: api/Reservations/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReservationById(int id)
            {
                var reservation = await _reservationService.GetReservationByIdAsync(id);
                if (reservation == null)
                {
                    return NotFound(new { message = "Reservation not found." });
                }

                var response = new
                {
                    FirstName = reservation.User?.FirstName ?? "N/A",
                    LastName = reservation.User?.LastName ?? "N/A",
                    Email = reservation.User?.Email ?? "N/A",
                    ReservationDetails = new
                    {
                        reservation.ReservationTime,
                        reservation.NumberOfGuests
                    }
                };

                return Ok(response);
            }  

        // GET: api/Reservations/user-reservations
        [HttpGet("user-reservations")]
        public async Task<IActionResult> GetReservationsByEmail([FromQuery] string email)
        {
            var reservations = await _reservationService.GetReservationsByEmailAsync(email);
            if (reservations == null || !reservations.Any())
            {
                return NotFound(new { message = "No reservations found for the given email." });
            }

            return Ok(reservations);
        }

        // PUT: api/Reservations/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReservation(int id, [FromBody] ReservationVM model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Map the ViewModel to the Model
                var reservation = _mapper.Map<Reservation>(model);
                reservation.ReservationId = id; // Ensure the ID is set correctly

                var updateResult = await _reservationService.UpdateReservationAsync(reservation); // Pass the mapped Reservation model
                if (!updateResult)
                {
                    return NotFound($"No reservation found with ID {id}.");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                // Log the exception here
                _logger.LogError(ex, "An error occurred while updating the reservation.");
                return StatusCode(500, "An error occurred while updating the reservation.");
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
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Error deleting reservation with ID {id}.");
                return NotFound($"Reservation with ID {id} not found.");
            }
        }
    }
}
