// using Microsoft.AspNetCore.Identity;
// using System.Collections.Generic;
// using System.ComponentModel.DataAnnotations;
// using System.ComponentModel.DataAnnotations.Schema;

// namespace RestaurantBackend.Models
// {
//     // If you want to customize the table name, that's fine
//     [Table("Customers")]
//     public class Customer : IdentityUser
//     {
//         [Required]
//         [MaxLength(50)]
//         public string FirstName { get; set; }  = string.Empty;

//         [Required]
//         [MaxLength(50)]
//         public string LastName { get; set; }  = string.Empty;

//         [Phone]
//         public string PhoneNo { get; set; }  = string.Empty;

//         // Fields for temporary password and its expiry
//         public string TemporaryPassword { get; set; } = string.Empty;
//         public DateTime? TemporaryPasswordExpiry { get; set; } 
//     }
// }
