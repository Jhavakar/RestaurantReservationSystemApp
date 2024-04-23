using AutoMapper;
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
                var reservation = _mapper.Map<Reservation>(model);
                var createdReservation = await _reservationService.CreateReservationAsync(reservation, model.EmailAddress);
                if (createdReservation == null)
                {
                    _logger.LogError("Failed to create reservation.");
                    return BadRequest("Failed to create the reservation.");
                }

                var responseModel = _mapper.Map<ReservationVM>(createdReservation);
                responseModel.EmailAddress = model.EmailAddress; // Ensure this is set properly

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
                _logger.LogWarning($"Reservation with ID {id} not found.");
                return NotFound();
            }
            return Ok(_mapper.Map<ReservationVM>(reservation));
        }

        // PUT: api/Reservations/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReservation(int id, [FromBody] ReservationVM model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != model.ReservationId)
            {
                return BadRequest("Mismatched reservation ID.");
            }

            try
            {
                var reservation = _mapper.Map<Reservation>(model);
                await _reservationService.UpdateReservationAsync(reservation);
                return NoContent();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error updating reservation.");
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
