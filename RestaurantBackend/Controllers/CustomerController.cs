using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RestaurantBackend.Models;
using RestaurantBackend.Services;
using RestaurantBackend.Utility;
using RestaurantBackend.ViewModels;

namespace RestaurantBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly IMapper _mapper;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(ICustomerService customerService, IMapper mapper, ILogger<CustomerController> logger)
        {
            _customerService = customerService;
            _mapper = mapper;
            _logger = logger;
        }

        // POST: api/Customer
        [HttpPost]
        public async Task<ActionResult<CustomerVM>> CreateCustomer([FromBody] CustomerVM customerVM)
        {
            var customer = _mapper.Map<Customer>(customerVM); // Map from CustomerVM to Customer for creation
            var newCustomerVM = await _customerService.AddCustomerAsync(customer); // Service expects Customer
            return CreatedAtAction(nameof(GetCustomer), new { id = newCustomerVM.CustomerId }, newCustomerVM);
        }

        // GET: api/Customer/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerVM>> GetCustomer(int id)
        {
            try
            {
                var customer = await _customerService.GetCustomerByIdAsync(id);

                if (customer == null)
                {
                    _logger.LogWarning($"Customer with ID {id} not found.");
                    return NotFound();
                }

                return _mapper.Map<CustomerVM>(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving customer with ID {id}.");
                throw;
            }
        }


        // GET: api/Customer
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerVM>>> GetAllCustomers()
        {
            var customers = await _customerService.GetAllCustomersAsync();
            return _mapper.Map<List<CustomerVM>>(customers);
        }

        // PUT: api/Customer/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] CustomerVM customerVM)
        {
            if (id != customerVM.CustomerId)
            {
                return BadRequest();
            }

            var customer = _mapper.Map<Customer>(customerVM); // Map from CustomerVM to Customer for update
            var updatedCustomerVM = await _customerService.UpdateCustomerAsync(customer); // Service expects Customer
            if (updatedCustomerVM == null) return NotFound();

            return NoContent(); // Alternatively, return the updated CustomerVM if that's your API design
        }
        // DELETE: api/Customer/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            await _customerService.DeleteCustomerAsync(id);
            return NoContent();
        }
    }
}
