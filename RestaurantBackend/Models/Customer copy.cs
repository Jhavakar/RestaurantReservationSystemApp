// using System.Collections.Generic;
// using System.ComponentModel.DataAnnotations;
// using System.ComponentModel.DataAnnotations.Schema;

// namespace RestaurantBackend.Models
// {
//     [Table("Customers")]
//     public class Customer
//     {
//         [Key]
//         [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
//         public int CustomerId { get; set; }

//         [Required]
//         [EmailAddress]
//         public string EmailAddress { get; set; }  = string.Empty;

//         [Required]
//         [MaxLength(50)]
//         public string FirstName { get; set; }  = string.Empty;

//         [Required]
//         [MaxLength(50)]
//         public string LastName { get; set; }  = string.Empty;

//         [Phone]
//         public string PhoneNo { get; set; }  = string.Empty;

//         public byte[] PasswordHash { get; set; }
//         public byte[] PasswordSalt { get; set; }

//         // Navigation properties
//         public virtual ICollection<Reservation> Reservations { get; set; } = new HashSet<Reservation>();
//     }
// }
