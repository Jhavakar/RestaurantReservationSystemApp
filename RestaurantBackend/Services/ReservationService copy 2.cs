// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Logging;
// using RestaurantBackend.Data;
// using RestaurantBackend.Models;
// using RestaurantBackend.ViewModels;
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Net;
// using System.Net.Mail;
// using System.Threading.Tasks;
// using System.Security.Cryptography;
// using Microsoft.Extensions.Configuration;  // Ensure this using directive is included for IConfiguration

// namespace RestaurantBackend.Services
// {
//     public interface IReservationService
//     {
//         Task<Reservation> CreateReservationAsync(Reservation reservation);
//         Task<Reservation> CreateReservationWithCustomerAsync(ReservationVM model);
//         Task<Reservation> GetReservationByIdAsync(int reservationId);
//         Task<IEnumerable<Reservation>> GetAllReservationsAsync();
//         Task UpdateReservationAsync(Reservation reservation);
//         Task CancelReservationAsync(int reservationId);
//     }

//     public class ReservationService : IReservationService
//     {
//         private readonly ApplicationDbContext _context;
//         private readonly ILogger<ReservationService> _logger;
//         private readonly IConfiguration _configuration;
//         private readonly ICustomerService _customerService;

//         public ReservationService(ApplicationDbContext context, ILogger<ReservationService> logger, IConfiguration configuration, ICustomerService customerService)
//         {
//             _context = context;
//             _logger = logger;
//             _configuration = configuration;
//             _customerService = customerService;
//         }

//         public async Task<Reservation> CreateReservationAsync(Reservation reservation)
//         {
//             reservation.ReservationEndTime = reservation.ReservationTime.AddHours(1);
//             _context.Reservations.Add(reservation);
//             await _context.SaveChangesAsync();
//             _logger.LogInformation("Reservation created successfully.");
//             return reservation;
//         }

//         public async Task<Reservation> CreateReservationWithCustomerAsync(ReservationVM model)
//         {
//             using (var transaction = await _context.Database.BeginTransactionAsync())
//             {
//                 try
//                 {
//                     var customer = await _customerService.GetCustomerByEmailAsync(model.Email);
//                     if (customer == null)
//                     {
//                         customer = new Customer
//                         {
//                             FirstName = model.FirstName,
//                             LastName = model.LastName,
//                             Email = model.Email.Trim(),
//                         };
//                         _context.Customers.Add(customer);
//                         await _context.SaveChangesAsync();
//                     }

//                     bool activeReservation = await _context.Reservations.AnyAsync(r =>
//                         r.CustomerId == customer.Id && r.ReservationEndTime > DateTime.Now);

//                     if (activeReservation)
//                     {
//                         throw new InvalidOperationException("This customer already has an active reservation.");
//                     }

//                     var reservation = new Reservation
//                     {
//                         CustomerId = customer.Id,
//                         ReservationTime = model.ReservationTime,
//                         ReservationEndTime = model.ReservationTime.AddHours(1),
//                         NumberOfGuests = model.NumberOfGuests,
//                     };

//                     _context.Reservations.Add(reservation);
//                     await _context.SaveChangesAsync();
//                     await transaction.CommitAsync();

//                     return reservation;
//                 }
//                 catch (Exception ex)
//                 {
//                     await transaction.RollbackAsync();
//                     _logger.LogError(ex, "Failed to create reservation.");
//                     throw;
//                 }
//             }
//         }

//         public async Task<Reservation> GetReservationByIdAsync(int reservationId)
//         {
//             return await _context.Reservations.FindAsync(reservationId);
//         }

//         public async Task<IEnumerable<Reservation>> GetAllReservationsAsync()
//         {
//             return await _context.Reservations.ToListAsync();
//         }

//         public async Task UpdateReservationAsync(Reservation reservation)
//         {
//             var existingReservation = await _context.Reservations.FindAsync(reservation.ReservationId);
//             if (existingReservation == null)
//             {
//                 _logger.LogWarning($"Reservation with ID {reservation.ReservationId} not found for update.");
//                 throw new KeyNotFoundException($"Reservation with ID {reservation.ReservationId} not found.");
//             }

//             _context.Entry(existingReservation).CurrentValues.SetValues(reservation);
//             await _context.SaveChangesAsync();
//             _logger.LogInformation($"Reservation updated: {reservation}");
//         }

//         public async Task CancelReservationAsync(int reservationId)
//         {
//             var reservation = await _context.Reservations.FindAsync(reservationId);
//             if (reservation != null)
//             {
//                 _context.Reservations.Remove(reservation);
//                 await _context.SaveChangesAsync();
//                 _logger.LogInformation("Reservation with ID {reservationId} deleted.", reservationId);
//             }
//             else
//             {
//                 _logger.LogWarning("Reservation with ID {reservationId} not found for deletion.", reservationId);
//             }
//         }

//         private async Task SendConfirmationEmail(Reservation reservation, Customer customer)
//         {
//             if (customer == null || string.IsNullOrEmpty(customer.Email))
//             {
//                 _logger.LogError("Invalid customer data. Email is missing.");
//                 return; // Exit if no valid customer email
//             }

//             try
//             {
//                 var smtpClient = new SmtpClient("smtp.gmail.com")
//                 {
//                     Port = 587,
//                     Credentials = new NetworkCredential(_configuration["EmailSettings:EmailAddress"], _configuration["EmailSettings:Password"]),
//                     EnableSsl = true,
//                 };

//                 using (var message = new MailMessage(_configuration["EmailSettings:EmailAddress"], customer.Email))
//                 {
//                     message.Subject = "Your reservation confirmation";
//                     message.Body = $"Dear {customer.FirstName},\n\n" +
//                                    $"Your reservation for {reservation.ReservationTime} with {reservation.NumberOfGuests} guests has been successfully made.\n\n" +
//                                    $"Your temporary password to manage your reservation is: {customer.TemporaryPassword}\n\n" +
//                                    $"Please use this password to log in and manage your reservation details. Note: This password will expire in 24 hours.\n\n" +
//                                    $"Best regards,\n" +
//                                    $"Your Restaurant Team";
//                     await smtpClient.SendMailAsync(message);
//                 }
//                 _logger.LogInformation("Confirmation email sent successfully to {Email}.", customer.Email);
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Failed to send confirmation email to {Email}.", customer.Email);
//             }
//         }

//     }
// }
