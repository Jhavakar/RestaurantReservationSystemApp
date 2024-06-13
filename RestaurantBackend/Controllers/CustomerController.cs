using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RestaurantBackend.Models;
using RestaurantBackend.Services;
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
        public async Task<IActionResult> CreateCustomer([FromBody] CustomerVM model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var customer = _mapper.Map<Customer>(model);
                var result = await _customerService.CreateCustomerAsync(model);

                if (result.Succeeded)
                {
                    var createdCustomerVM = _mapper.Map<CustomerVM>(customer);
                    return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, createdCustomerVM);
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return BadRequest(ModelState);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create customer.");
                return StatusCode(500, "Internal server error while creating customer.");
            }
        }

        // GET: api/Customer/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomer(string id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                return NotFound($"Customer with ID {id} not found.");
            }

            var customerVM = new CustomerVM
            {
                Id = customer.Id,  // Ensure the Id is included in the response
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                PhoneNumber = customer.PhoneNumber
            };

            return Ok(customerVM);
        }

        // GET: api/Customer/email/{email}
        [HttpGet("email/{email}")]
        public async Task<IActionResult> GetCustomerByEmail(string email)
        {
            var customer = await _customerService.GetCustomerByEmailAsync(email);
            if (customer == null)
            {
                return NotFound($"Customer with Email {email} not found.");
            }

            var customerVM = new CustomerVM
            {
                Id = customer.Id,  // Ensure the Id is included in the response
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                PhoneNumber = customer.PhoneNumber
            };

            return Ok(customerVM);
        }

        // PUT: api/Customer/email/{email}
        [HttpPut("email/{email}")]
        public async Task<IActionResult> UpdateCustomerByEmail(string email, [FromBody] CustomerVM model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _customerService.UpdateCustomerByEmailAsync(email, model);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"Customer with email {email} updated successfully.");
                    return NoContent(); // 204 No Content is typically returned when an update operation successfully completes.
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return BadRequest(ModelState);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating customer with email {email}.");
                return StatusCode(500, "Internal server error while updating customer.");
            }
        }

        // DELETE: api/Customer/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(string id)
        {
            try
            {
                var result = await _customerService.DeleteCustomerAsync(id);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"Customer with ID {id} deleted successfully.");
                    return NoContent(); // 204 No Content is a common response for successful DELETE operations.
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return BadRequest(ModelState);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting customer with ID {id}.");
                return StatusCode(500, "Internal server error while deleting customer.");
            }
        }
    }

}
