// using System;
// using System.IdentityModel.Tokens.Jwt;
// using System.Security.Claims;
// using System.Text;
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
// using System.Threading.Tasks;

// namespace RestaurantBackend.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     [EnableCors("MyPolicy")]
//     public class CustomerController : ControllerBase
//     {
//         private readonly ICustomerService _customerService;
//         private readonly IMapper _mapper;
//         private readonly AppSettings _appSettings;
//         private readonly ILogger<CustomerController> _logger;

//         public CustomerController(
//             ICustomerService customerService,
//             IMapper mapper,
//             IOptions<AppSettings> appSettings,
//             ILogger<CustomerController> logger)
//         {
//             _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
//             _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
//             _appSettings = appSettings.Value ?? throw new ArgumentNullException(nameof(appSettings));
//             _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//         }

//        [AllowAnonymous]
//         [HttpPost("register")]
//         public async Task<IActionResult> Create(CustomerVM model)
//         {
//             if (!ModelState.IsValid)
//             {
//                 return BadRequest(model);
//             }

//             try
//             {
//                 var createdModel = await _customerService.CreateCustomerAsync(model);
//                 return Ok("Registration successful");
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Unexpected error occurred during registration.");
//                 return StatusCode(500, "An unexpected error occurred. Please try again later.");
//             }
//         }

//         [HttpPost("authenticate")]
//         public async Task<IActionResult> Authenticate([FromBody] CustomerVM model)
//         {
//             var customer = await _customerService.Authenticate(model.EmailAddress, model.Password);
//             if (customer == null) return BadRequest(new { message = "Email or password is incorrect" });

//             var tokenString = GenerateJwtToken(customer);
//             return Ok(new
//             {
//                 Id = customer.CustomerId,
//                 EmailAddress = customer.EmailAddress,
//                 FirstName = customer.FirstName,
//                 LastName = customer.LastName,
//                 Token = tokenString
//             });
//         }

//         private string GenerateJwtToken(Customer customer)
//         {
//             var tokenHandler = new JwtSecurityTokenHandler();
//             var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
//             var tokenDescriptor = new SecurityTokenDescriptor
//             {
//                 Subject = new ClaimsIdentity(new Claim[] 
//                 {
//                     new Claim(ClaimTypes.Name, customer.CustomerId.ToString())
//                 }),
//                 Expires = DateTime.UtcNow.AddDays(7),
//                 SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
//             };

//             var token = tokenHandler.CreateToken(tokenDescriptor);
//             return tokenHandler.WriteToken(token);
//         }

//         [HttpGet("reservations/{customerId}")]
//         public async Task<IActionResult> GetCustomerReservations(int customerId)
//         {
//             try
//             {
//                 var reservations = await _customerService.GetCustomerReservationsAsync(customerId);
//                 return Ok(reservations);
//             }
//             catch (ApplicationException ex)
//             {
//                 _logger.LogError(ex, "Failed to retrieve reservations for customer ID: {CustomerId}", customerId);
//                 return BadRequest(new { message = ex.Message });
//             }
//         }

//         // Consider adding more methods here as necessary, like updating customer details or password.
//     }
// }
