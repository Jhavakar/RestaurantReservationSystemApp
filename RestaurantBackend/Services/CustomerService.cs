using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantBackend.Data;
using RestaurantBackend.Models;
using RestaurantBackend.ViewModels;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantBackend.Services
{
    public interface ICustomerService
    {
        Task<CustomerVM> AddCustomerAsync(Customer customer); // Use Customer for adding
        Task<CustomerVM> GetCustomerByIdAsync(int customerId); // Retrieval uses CustomerVM
        Task<List<CustomerVM>> GetAllCustomersAsync(); // Retrieval uses CustomerVM
        Task<CustomerVM> UpdateCustomerAsync(Customer customer); // Use Customer for updating
        Task DeleteCustomerAsync(int customerId);
    }


    public class CustomerService : ICustomerService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CustomerService> _logger;
        private readonly IMapper _mapper;

        public CustomerService(ApplicationDbContext context, ILogger<CustomerService> logger, IMapper mapper)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
        }

        // Create (using CustomerVM)
        public async Task<CustomerVM> AddCustomerAsync(Customer customer)
        {
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            return _mapper.Map<CustomerVM>(customer); // Return as CustomerVM after add
        }

        // Read (single customer)
        public async Task<CustomerVM> GetCustomerByIdAsync(int customerId)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            return _mapper.Map<CustomerVM>(customer);
        }

        // Read (all customers)
        public async Task<List<CustomerVM>> GetAllCustomersAsync()
        {
            var customers = await _context.Customers.ToListAsync();
            return _mapper.Map<List<CustomerVM>>(customers);
        }

        // Update (using CustomerVM as this might involve updating any customer information)
        public async Task<CustomerVM> UpdateCustomerAsync(Customer customer)
        {
            var existingCustomer = await _context.Customers.FindAsync(customer.CustomerId);
            if (existingCustomer == null) return null; // Handle not found scenario
            
            _context.Entry(existingCustomer).CurrentValues.SetValues(customer);
            await _context.SaveChangesAsync();
            return _mapper.Map<CustomerVM>(existingCustomer); // Return as CustomerVM after update
        }

        // Delete
        public async Task DeleteCustomerAsync(int customerId)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
            }
        }
    }
}
