using System;
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
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return BadRequest(ModelState);
                }
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
            return Ok(customer);
        }
        
        // PUT: api/Customer/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(string id, [FromBody] CustomerVM model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _customerService.UpdateCustomerAsync(id, model);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"Customer with ID {id} updated successfully.");
                    return NoContent(); // 204 No Content is typically returned when an update operation successfully completes.
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return BadRequest(ModelState);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating customer with ID {id}.");
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
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return BadRequest(ModelState);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting customer with ID {id}.");
                return StatusCode(500, "Internal server error while deleting customer.");
            }
        }

        [HttpPost("updatePassword/{userId}")]
        public async Task<IActionResult> UpdatePassword(string userId, [FromBody] string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
            {
                return BadRequest("New password is required");
            }

            var result = await _customerService.UpdatePasswordAsync(userId, newPassword);
            if (result.Succeeded)
            {
                return Ok("Password updated successfully.");
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }

   }
}
