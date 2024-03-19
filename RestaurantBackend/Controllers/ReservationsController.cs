using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization; 
using RestaurantBackend.Data;
using RestaurantBackend.Models;
using RestaurantBackend.ViewModels;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RestaurantBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReservationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ReservationsController> _logger;

        public ReservationsController(ApplicationDbContext context, IMapper mapper, ILogger<ReservationsController> logger)
        {
            _context = context;
            _mapper = mapper;            
            _logger = logger;

        }

        // GET: api/Reservations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Reservation>>> GetReservations()
        {
            return await _context.Reservations.ToListAsync();
        }

        // GET: api/Reservations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Reservation>> GetReservation(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }
            return reservation;
        }

        // POST: api/Reservations
        [HttpPost]
        public async Task<ActionResult<Reservation>> PostReservation(Reservation reservation)
        {
            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetReservation), new { id = reservation.Id }, reservation);
        }

        // PUT: api/Reservations/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReservation(int id, Reservation reservation)
        {
            if (id != reservation.Id)
            {
                return BadRequest();
            }
            _context.Entry(reservation).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Reservations.Any(e => e.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Reservations/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReservation(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }
            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // GET: api/Reservations/Search/{searchTerm}
        [HttpGet("Search/{searchTerm}")]
        public async Task<ActionResult<IEnumerable<Reservation>>> SearchReservations(string searchTerm)
        {
            var reservations = await _context.Reservations
                .Include(r => r.Customer) // Include the Customer entity
                .Where(r => (r.Customer.FirstName + " " + r.Customer.LastName).Contains(searchTerm))
                .ToListAsync();
            return Ok(reservations);
        }

        // GET: api/Reservations/AvailableSlots/{tableId}
        [HttpGet("AvailableSlots/{tableId}")]
        public async Task<ActionResult<IEnumerable<DateTime>>> GetAvailableReservationSlots(int tableId, [FromQuery] DateTime date)
        {
            var reservations = await _context.Reservations
                .Where(r => r.TableId == tableId && r.ReservationTime.Date == date.Date)
                .Select(r => r.ReservationTime)
                .ToListAsync();

            // Assuming reservations are for 1 hour each and the restaurant operates from 12:00 to 23:00
            var slots = new List<DateTime>();
            for (int hour = 12; hour <= 22; hour++) // Adjust according to your business hours
            {
                var potentialSlot = new DateTime(date.Year, date.Month, date.Day, hour, 0, 0);
                if (!reservations.Contains(potentialSlot))
                {
                    slots.Add(potentialSlot);
                }
            }

            return Ok(slots);
        }
    
        [HttpPost("Create")]
        public async Task<IActionResult> CreateReservation([FromBody] ReservationVM reservationVM)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var reservation = _mapper.Map<Reservation>(reservationVM);
                _context.Reservations.Add(reservation);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetReservation), new { id = reservation.Id }, reservationVM);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create reservation");
                return BadRequest(new { message = ex.Message });
            }
        }

        // Your method to get customer's reservations...
        [HttpGet("my-reservations")]
        public async Task<IActionResult> GetMyReservations()
        {
            if (User.Identity?.Name == null)
            {
                return Unauthorized("User is not logged in.");
            }

            int customerId = int.Parse(User.Identity.Name);
            var reservations = await _context.Reservations
                                             .Where(r => r.CustomerId == customerId)
                                             .ToListAsync();
            
            return Ok(reservations);
        }
    }
    
}
