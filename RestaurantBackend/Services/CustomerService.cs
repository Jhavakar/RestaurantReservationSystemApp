using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using RestaurantBackend.Models;
using RestaurantBackend.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantBackend.Services
{
    public interface ICustomerService
    {
        Task<IdentityResult> CreateCustomerAsync(CustomerVM model);
        Task<IdentityResult> UpdateCustomerByEmailAsync(string email, CustomerVM model);
        Task<IdentityResult> DeleteCustomerAsync(string customerId);
        Task<CustomerVM?> GetCustomerByIdAsync(string customerId);
        Task<CustomerVM?> GetCustomerByEmailAsync(string email);
    }

    public class CustomerService : ICustomerService
    {
        private readonly UserManager<Customer> _userManager;
        private readonly ILogger<CustomerService> _logger;

        public CustomerService(UserManager<Customer> userManager, ILogger<CustomerService> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IdentityResult> CreateCustomerAsync(CustomerVM model)
        {
            var customer = new Customer
            {
                Email = model.Email,
                UserName = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber
            };

            if (string.IsNullOrWhiteSpace(customer.UserName) || string.IsNullOrWhiteSpace(customer.Email))
            {
                _logger.LogError("Username or Email is empty. Username and Email must be provided.");
                return IdentityResult.Failed(new IdentityError { Description = "Username or Email is empty." });
            }

            var result = await _userManager.CreateAsync(customer, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    _logger.LogError($"Error creating user: {error.Description}");
                }
            }
            return result;
        }

        public async Task<IdentityResult> UpdateCustomerByEmailAsync(string email, CustomerVM model)
        {
            var customer = await _userManager.FindByEmailAsync(email);
            if (customer == null || string.IsNullOrWhiteSpace(customer.Id))
            {
                _logger.LogError($"No customer found with email {email} or customer ID is missing");
                return IdentityResult.Failed(new IdentityError { Description = "Customer not found or ID is missing." });
            }

            model.Id = customer.Id;  

            customer.FirstName = model.FirstName;
            customer.LastName = model.LastName;
            customer.PhoneNumber = model.PhoneNumber;
            customer.Email = model.Email;
            customer.UserName = model.Email;

            var result = await _userManager.UpdateAsync(customer);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    _logger.LogError($"Error updating customer: {error.Description}");
                }
            }
            return result;
        }

        public async Task<IdentityResult> DeleteCustomerAsync(string customerId)
        {
            var customer = await _userManager.FindByIdAsync(customerId);
            if (customer == null)
            {
                _logger.LogError($"No customer found with ID {customerId}");
                return IdentityResult.Failed(new IdentityError { Description = "Customer not found." });
            }

            var result = await _userManager.DeleteAsync(customer);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    _logger.LogError($"Error deleting customer: {error.Description}");
                }
            }
            return result;
        }

        public async Task<CustomerVM?> GetCustomerByIdAsync(string customerId)
        {
            var customer = await _userManager.FindByIdAsync(customerId);
            if (customer == null)
            {
                _logger.LogWarning($"Customer ID {customerId} not found.");
                return null;
            }

            return new CustomerVM
            {
                Id = customer.Id,  
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                PhoneNumber = customer.PhoneNumber
            };
        }

        public async Task<CustomerVM?> GetCustomerByEmailAsync(string email)
        {
            var customer = await _userManager.FindByEmailAsync(email);
            if (customer == null)
            {
                _logger.LogError($"No customer found with email {email}");
                return null;
            }

            return new CustomerVM
            {
                Id = customer.Id,  
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                PhoneNumber = customer.PhoneNumber
            };
        }
    }

}
