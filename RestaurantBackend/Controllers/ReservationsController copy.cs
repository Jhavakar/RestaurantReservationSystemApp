// using System;
// using System.IdentityModel.Tokens.Jwt;
// using System.Collections.Generic;
// using System.Linq;
// using System.Security.Claims;
// using System.Text;
// using System.Threading.Tasks;
// using AutoMapper;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Cors;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.Extensions.Logging;
// using Microsoft.Extensions.Options;
// using Microsoft.IdentityModel.Tokens;
// using RestaurantBackend.Models;
// using RestaurantBackend.Services;
// using RestaurantBackend.Utility;
// using RestaurantBackend.ViewModels;

// namespace RestaurantBackend.Controllers
// {
//     [Route("api/[controller]")]
//     [ApiController]
//     [Authorize]
//     public class ReservationsController : ControllerBase
//     {
//         private readonly IReservationService _reservationService;
//         private readonly IMapper _mapper;
//         private readonly ILogger<ReservationsController> _logger;

//         public ReservationsController(IReservationService reservationService, IMapper mapper, ILogger<ReservationsController> logger)
//         {
//             _reservationService = reservationService;
//             _mapper = mapper;            
//             _logger = logger;
//         }

//         // POST: api/Reservations
//         [HttpPost]
//         public async Task<IActionResult> CreateReservation([FromBody] ReservationDto reservationDto)
//         {
//             if (!ModelState.IsValid)
//             {
//                 return BadRequest(ModelState);
//             }

//             var reservation = _mapper.Map<Reservation>(reservationDto); // Assuming AutoMapper setup is correctly configured
//             var createdReservation = await _reservationService.CreateReservationAsync(reservation);
//             if (createdReservation == null)
//             {
//                 _logger.LogError("Unable to create reservation, possible conflicts or validation failure.");
//                 return BadRequest("Unable to create reservation, possible conflicts.");
//             }

//             return CreatedAtAction(nameof(GetReservationById), new { id = createdReservation.Id }, createdReservation);
//         }

//         // GET: api/Reservations
//         [HttpGet]
//         public async Task<ActionResult<IEnumerable<Reservation>>> GetReservations()
//         {
//             var reservations = await _reservationService.GetAllReservationsAsync();
//             return Ok(reservations);
//         }

//         // GET: api/Reservations/{id}
//         [HttpGet("{id}")]
//         public async Task<IActionResult> GetReservationById(int id)
//         {
//             var reservation = await _reservationService.GetReservationByIdAsync(id);

//             if (reservation == null)
//             {
//                 _logger.LogWarning($"Reservation with ID {id} not found.");
//                 return NotFound();
//             }

//             return Ok(reservation);
//         }

//         // PUT: api/Reservations/{id}
//         [HttpPut("{id}")]
//         public async Task<IActionResult> UpdateReservation(int id, [FromBody] ReservationDto reservationDto)
//         {
//             if (!ModelState.IsValid)
//             {
//                 return BadRequest(ModelState);
//             }

//             if (id != reservationDto.Id)
//             {
//                 return BadRequest("Mismatched reservation ID.");
//             }

//             var reservation = _mapper.Map<Reservation>(reservationDto);
//             var updatedReservation = await _reservationService.UpdateReservationAsync(reservation);

//             if (updatedReservation == null)
//             {
//                 return NotFound($"Reservation with ID {id} not found.");
//             }

//             return NoContent();
//         }

//         // DELETE: api/Reservations/{id}
//         [HttpDelete("{id}")]
//         public async Task<IActionResult> DeleteReservation(int id)
//         {
//             var success = await _reservationService.CancelReservationAsync(id);
//             if (!success)
//             {
//                 return NotFound($"Reservation with ID {id} not found.");
//             }

//             return NoContent();
//         }
//         // GET: api/Reservations/Search/{searchTerm}
//         // [HttpGet("Search/{searchTerm}")]
//         // public async Task<ActionResult<IEnumerable<Reservation>>> SearchReservations(string searchTerm)
//         // {
//         //     var reservations = await _context.Reservations
//         //         .Include(r => r.Customer) // Include the Customer entity
//         //         .Where(r => (r.Customer.FirstName + " " + r.Customer.LastName).Contains(searchTerm))
//         //         .ToListAsync();
//         //     return Ok(reservations);
//         // }

//         // GET: api/Reservations/AvailableSlots/{tableId}
//         // [HttpGet("AvailableSlots/{tableId}")]
//         // public async Task<ActionResult<IEnumerable<DateTime>>> GetAvailableReservationSlots(int tableId, [FromQuery] DateTime date)
//         // {
//         //     var reservations = await _context.Reservations
//         //         .Where(r => r.TableId == tableId && r.ReservationTime.Date == date.Date)
//         //         .Select(r => r.ReservationTime)
//         //         .ToListAsync();

//         //     // Assuming reservations are for 1 hour each and the restaurant operates from 12:00 to 23:00
//         //     var slots = new List<DateTime>();
//         //     for (int hour = 12; hour <= 22; hour++) // Adjust according to your business hours
//         //     {
//         //         var potentialSlot = new DateTime(date.Year, date.Month, date.Day, hour, 0, 0);
//         //         if (!reservations.Contains(potentialSlot))
//         //         {
//         //             slots.Add(potentialSlot);
//         //         }
//         //     }

//         //     return Ok(slots);
//         // }
    
//         // Your method to get customer's reservations...
//         // [HttpGet("my-reservations")]
//         // public async Task<IActionResult> GetMyReservations()
//         // {
//         //     if (User.Identity?.Name == null)
//         //     {
//         //         return Unauthorized("User is not logged in.");
//         //     }

//         //     int customerId = int.Parse(User.Identity.Name);
//         //     var reservations = await _context.Reservations
//         //                                      .Where(r => r.CustomerId == customerId)
//         //                                      .ToListAsync();
            
//         //     return Ok(reservations);
//         // }
    
//     }
    
// }
