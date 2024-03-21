// using AutoMapper;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Logging;
// using RestaurantBackend.Data;
// using RestaurantBackend.Models;
// using RestaurantBackend.ViewModels;
// using System;
// using System.Collections.Generic;
// using System.Security.Cryptography;
// using System.Text;
// using System.Threading.Tasks;

// namespace RestaurantBackend.Services
// {
//     public interface ICustomerService
//     {
//         Task<CustomerVM> CreateCustomerAsync(CustomerVM customerVM);
//         Task<Customer> Authenticate(string email, string password);
//         Task<bool> CanCustomerCreateReservationAsync(int customerId);
//         Task CancelReservationAsync(int reservationId);
//         Task CreateReservationAsync(Reservation reservation);
//         Task<IEnumerable<Reservation>> GetCustomerReservationsAsync(int customerId);
//     }

//     public class CustomerService : ICustomerService
//     {
//         private readonly ApplicationDbContext _context;
//         private readonly ILogger<CustomerService> _logger;
//         private readonly IMapper _mapper;

//         public CustomerService(ApplicationDbContext context, ILogger<CustomerService> logger, IMapper mapper)
//         {
//             _context = context ?? throw new ArgumentNullException(nameof(context));
//             _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//             _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
//         }

//         public async Task<CustomerVM> CreateCustomerAsync(CustomerVM model)
//         {
//             var customerExists = await _context.Customers.AnyAsync(c => c.EmailAddress == model.EmailAddress);
//             if (customerExists)
//             {
//                 throw new ApplicationException("Email is already registered.");
//             }

//             var customer = _mapper.Map<Customer>(model);
//             _context.Customers.Add(customer);
//             await _context.SaveChangesAsync();
//             return _mapper.Map<CustomerVM>(customer); // Ensure this matches your intended use
//         }


//         public async Task<Customer> Authenticate(string email, string password)
//         {
//             var customer = await _context.Customers.FirstOrDefaultAsync(x => x.EmailAddress == email);
//             if (customer == null || !VerifyPasswordHash(password, customer.PasswordHash, customer.PasswordSalt))
//             {
//                 return null;
//             }
//             return customer;
//         }


//         public async Task<bool> CanCustomerCreateReservationAsync(int customerId)
//         {
//             try
//             {
//                 var hasActiveReservation = await _context.Reservations
//                                                          .AnyAsync(r => r.CustomerId == customerId && r.IsActive);
//                 return !hasActiveReservation;
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error checking reservation availability for customer {CustomerId}", customerId);
//                 throw;
//             }
//         }

//         public async Task CreateReservationAsync(Reservation reservation)
//         {
//             if (await CanCustomerCreateReservationAsync(reservation.CustomerId))
//             {
//                 try
//                 {
//                     await _context.Reservations.AddAsync(reservation);
//                     await _context.SaveChangesAsync();
//                     _logger.LogInformation("Reservation created for customer {CustomerId}", reservation.CustomerId);
//                 }
//                 catch (Exception ex)
//                 {
//                     _logger.LogError(ex, "Error creating reservation for customer {CustomerId}", reservation.CustomerId);
//                     throw;
//                 }
//             }
//             else
//             {
//                 _logger.LogWarning("Attempted to create a reservation for customer {CustomerId} who already has an active reservation.", reservation.CustomerId);
//                 throw new InvalidOperationException("Customer already has an active reservation.");
//             }
//         }

//         public async Task<IEnumerable<Reservation>> GetCustomerReservationsAsync(int customerId)
//     {
//         try
//         {
//             var reservations = await _context.Reservations
//                 .Where(reservation => reservation.CustomerId == customerId)
//                 .ToListAsync();

//             return reservations;
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "An error occurred while retrieving reservations for customer with ID {CustomerId}", customerId);
//             throw;
//         }
//     }

//         public async Task CancelReservationAsync(int reservationId)
//         {
//             var reservation = await _context.Reservations.FindAsync(reservationId);
//             if (reservation != null && reservation.IsActive)
//             {
//                 try
//                 {
//                     reservation.IsActive = false;
//                     await _context.SaveChangesAsync();
//                     _logger.LogInformation("Reservation {ReservationId} cancelled.", reservationId);
//                 }
//                 catch (Exception ex)
//                 {
//                     _logger.LogError(ex, "Error cancelling reservation {ReservationId}", reservationId);
//                     throw;
//                 }
//             }
//             else
//             {
//                 _logger.LogWarning("Attempted to cancel a reservation {ReservationId} that is either not found or already inactive.", reservationId);
//                 throw new ApplicationException("Reservation not found or already cancelled.");
//             }
//         }

//         private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
//         {
//             using (var hmac = new HMACSHA512())
//             {
//                 passwordSalt = hmac.Key;
//                 passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
//             }
//         }

//         private bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
//         {
//             using (var hmac = new HMACSHA512(storedSalt))
//             {
//                 var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
//                 for (int i = 0; i < computedHash.Length; i++)
//                 {
//                     if (computedHash[i] != storedHash[i]) return false;
//                 }
//                 return true;
//             }
//         }
//     }
// }
