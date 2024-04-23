// using System.Collections.Generic;
// using System.ComponentModel.DataAnnotations;
// using System.ComponentModel.DataAnnotations.Schema;

// namespace RestaurantBackend.Models
// {
//     [Table("Users")]
//     public class Login
//     {
//         [Key]
//         [Required]
//         [EmailAddress]
//         public string EmailAddress { get; set; }  = string.Empty;

//         [Required]
//         [DataType(DataType.Password)]
//         public string PasswordHash { get; set; }  = string.Empty;

//         public int? CustomerId { get; set; }
//         [ForeignKey("CustomerId")]
//         public virtual Customer Customer { get; set; } 
//     }
// }
