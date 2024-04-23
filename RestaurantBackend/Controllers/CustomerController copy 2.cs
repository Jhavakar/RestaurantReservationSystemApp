// using System;
// using System.Collections.Generic;
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
//     public class CustomerController : ControllerBase
//     {
//         private readonly ICustomerService _customerService;
//         private readonly IMapper _mapper;
//         private readonly ILogger<CustomerController> _logger;

//         public CustomerController(ICustomerService customerService, IMapper mapper, ILogger<CustomerController> logger)
//         {
//             _customerService = customerService;
//             _mapper = mapper;
//             _logger = logger;
//         }

//         // POST: api/Customer
//         [HttpPost]
//         public async Task<ActionResult<CustomerVM>> CreateCustomer([FromBody] CustomerVM customerVM)
//         {
//             if (!ModelState.IsValid)
//             {
//                 _logger.LogWarning("ModelState is invalid: {ModelStateErrors}", ModelState.Values.SelectMany(v => v.Errors));
//                 return BadRequest(ModelState);
//             }

//             try
//             {
//                 var customer = _mapper.Map<Customer>(customerVM);
//                 var newCustomer = await _customerService.AddCustomerAsync(customer);
//                 var newCustomerVM = _mapper.Map<CustomerVM>(newCustomer);

//                 return CreatedAtAction(nameof(GetCustomer), new { id = newCustomerVM.Id }, newCustomerVM);
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Unexpected error occurred during registration.");
//                 return StatusCode(500, "An unexpected error occurred. Please try again later.");
//             }
//         }

//         // GET: api/Customer
//         [HttpGet]
//         public async Task<ActionResult<IEnumerable<CustomerVM>>> GetAllCustomers()
//         {
//             var customers = await _customerService.GetAllCustomersAsync();
//             return _mapper.Map<List<CustomerVM>>(customers);
//         }

//         // GET: api/Customer/{id}
//         [HttpGet("{id}")]
//         public async Task<ActionResult<CustomerVM>> GetCustomer(string id)
//         {
//             try
//             {
//                 var customer = await _customerService.GetCustomerByIdAsync(id);

//                 if (customer == null)
//                 {
//                     _logger.LogWarning($"Customer with ID {id} not found.");
//                     return NotFound();
//                 }

//                 return _mapper.Map<CustomerVM>(customer);
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, $"An error occurred while retrieving customer with ID {id}.");
//                 throw;
//             }
//         }

//         // PUT: api/Customer/{id}
//         [HttpPut("{id}")]
//         public async Task<IActionResult> UpdateCustomer(string id, [FromBody] CustomerVM customerVM)
//         {
//             if (id != customerVM.Id)
//             {
//                 return BadRequest("ID mismatch");
//             }

//             if (!ModelState.IsValid)
//             {
//                 return BadRequest(ModelState);
//             }

//             var existingCustomer = await _customerService.GetCustomerByIdAsync(id);
//             if (existingCustomer == null)
//             {
//                 return NotFound($"Customer with ID {id} not found.");
//             }

//             // If the customer exists, map the updated fields from customerVM to the existing customer
//             _mapper.Map(customerVM, existingCustomer);

//             // Update the customer using the service
//             var updatedCustomer = await _customerService.UpdateCustomerAsync(existingCustomer);

//             // Optionally, map the updated customer back to a CustomerVM to return to the client
//             var updatedCustomerVM = _mapper.Map<CustomerVM>(updatedCustomer);

//             return Ok(updatedCustomerVM);
//         }

//         // DELETE: api/Customer/{id}
//         [HttpDelete("{id}")]
//         public async Task<IActionResult> DeleteCustomer(string id)
//         {
//             await _customerService.DeleteCustomerAsync(id);
//             return NoContent();
//         }
//     }
// }
