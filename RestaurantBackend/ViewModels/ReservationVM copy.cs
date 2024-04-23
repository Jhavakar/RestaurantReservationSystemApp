// using System;
// using System.ComponentModel.DataAnnotations;

// namespace RestaurantBackend.ViewModels
// {
//     public class ReservationVM
//     {
//         public int? ReservationId { get; set; }  // Nullable for creation scenarios

//         [Required]
//         public DateTime ReservationTime { get; set; }

//         [Required]
//         [MaxLength(50)]
//         public string FirstName { get; set; }

//         [Required]
//         [MaxLength(50)]
//         public string LastName { get; set; }

//         [Required]
//         [EmailAddress]
//         public string Email { get; set; }  // Ensure this matches the JSON property "emailAddress" if changed

//         public string Id { get; set; }  // Should be string if using IdentityUser ID

//         [Required, Range(1, int.MaxValue, ErrorMessage = "The number of guests must be at least 1.")]
//         public int NumberOfGuests { get; set; }

//     }
// }
