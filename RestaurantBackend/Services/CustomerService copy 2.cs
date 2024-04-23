// using Microsoft.AspNetCore.Identity;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Logging;
// using RestaurantBackend.Data;
// using RestaurantBackend.Models;
// using RestaurantBackend.ViewModels;
// using System;
// using System.Collections.Generic;
// using System.Security.Cryptography;
// using System.Text;
// using System.Linq;
// using System.Threading.Tasks;

// namespace RestaurantBackend.Services
// {
//     public interface ICustomerService
//     {
//         Task<Customer> AddCustomerAsync(Customer customer);
//         Task<Customer> GetCustomerByIdAsync(string customerId);
//         Task<IEnumerable<Customer>> GetAllCustomersAsync();
//         Task<Customer> GetCustomerByEmailAsync(string email);
//         Task<bool> UpdateCustomerAsync(Customer customer);
//         Task DeleteCustomerAsync(string customerId);  // Adjusted to string for consistency with IdentityUser ID type
//         Task<bool> AuthenticateUserAsync(string email, string password);
//         Task SetPasswordAsync(Customer customer, string newPassword);
//     }

//     public class CustomerService : ICustomerService
//     {
//         private readonly ApplicationDbContext _context;
//         private readonly ILogger<CustomerService> _logger;
//         private readonly IPasswordHasher<Customer> _passwordHasher;

//         public CustomerService(ApplicationDbContext context, ILogger<CustomerService> logger, IPasswordHasher<Customer> passwordHasher)
//         {
//             _context = context;
//             _logger = logger;
//             _passwordHasher = passwordHasher;
//         }

//         public async Task<Customer> AddCustomerAsync(Customer customer)
//         {
//             if (!string.IsNullOrEmpty(customer.PasswordHash))
//             {
//                 customer.PasswordHash = _passwordHasher.HashPassword(customer, customer.PasswordHash);
//             }
//             _context.Customers.Add(customer);
//             await _context.SaveChangesAsync();
//             _logger.LogInformation($"Customer added: {customer}");
//             return customer;
//         }

//         public async Task<Customer> GetCustomerByIdAsync(string customerId)
//         {
//             return await _context.Customers.FindAsync(customerId);
//         }

//         public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
//         {
//             return await _context.Customers.ToListAsync();
//         }

//         public async Task<Customer> GetCustomerByEmailAsync(string email)
//         {
//             return await _context.Customers.FirstOrDefaultAsync(c => c.Email == email.Trim());
//         }

//         public async Task<bool> UpdateCustomerAsync(Customer customer)
//         {
//             var existingCustomer = await _context.Customers.FindAsync(customer.Id);
//             if (existingCustomer == null)
//             {
//                 _logger.LogWarning($"Customer with ID {customer.Id} not found for update.");
//                 return false;
//             }

//             _context.Entry(existingCustomer).CurrentValues.SetValues(customer);
//             try
//             {
//                 await _context.SaveChangesAsync();
//                 _logger.LogInformation($"Customer updated: {customer}");
//                 return true;
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, $"Failed to update customer with ID {customer.Id}.");
//                 return false;
//             }
//         }

//         public async Task DeleteCustomerAsync(string customerId)
//         {
//             var customer = await _context.Customers.FindAsync(customerId);
//             if (customer != null)
//             {
//                 _context.Customers.Remove(customer);
//                 await _context.SaveChangesAsync();
//                 _logger.LogInformation($"Customer with ID {customerId} deleted.");
//             }
//             else
//             {
//                 _logger.LogWarning($"Customer with ID {customerId} not found for deletion.");
//             }
//         }

//         public async Task<bool> AuthenticateUserAsync(string email, string password)
//         {
//             var customer = await GetCustomerByEmailAsync(email);
//             if (customer != null && _passwordHasher.VerifyHashedPassword(customer, customer.PasswordHash, password) == PasswordVerificationResult.Success)
//             {
//                 _logger.LogInformation("Authentication successful for user: {Email}", email);
//                 return true;
//             }
//             _logger.LogWarning("Authentication failed for user: {Email}", email);
//             return false;
//         }

//         public async Task SetPasswordAsync(Customer customer, string newPassword)
//         {
//             customer.PasswordHash = _passwordHasher.HashPassword(customer, newPassword);
//             _context.Customers.Update(customer);
//             await _context.SaveChangesAsync();
//         }

//         public async Task<Customer> CreateOrUpdateCustomerWithTemporaryPassword(ReservationVM model)
//         {
//             var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == model.Email.Trim());

//             string tempPassword = GenerateTemporaryPassword();
//             string hashedPassword = _passwordHasher.HashPassword(customer, tempPassword);

//             if (customer == null)
//             {
//                 customer = new Customer
//                 {
//                     FirstName = model.FirstName,
//                     LastName = model.LastName,
//                     Email = model.Email.Trim(),
//                     TemporaryPassword = hashedPassword,
//                     TemporaryPasswordExpiry = DateTime.UtcNow.AddHours(24) // Password expires in 24 hours
//                 };
//                                 _context.Customers.Add(customer);
//             }
//             else
//             {
//                 customer.TemporaryPassword = hashedPassword;
//                 customer.TemporaryPasswordExpiry = DateTime.UtcNow.AddHours(24);
//             }

//             await _context.SaveChangesAsync();
//             return customer;
//         }

//         private static string GenerateTemporaryPassword(int length = 8)
//         {
//             const string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZ0123456789";
//             using var rng = RandomNumberGenerator.Create();
//             var bytes = new byte[length];
//             rng.GetBytes(bytes);
//             var chars = new char[length];
//             for (int i = 0; i < length; i++)
//             {
//                 chars[i] = validChars[bytes[i] % validChars.Length];
//             }
//             return new string(chars);
//         }
//     }
// }

