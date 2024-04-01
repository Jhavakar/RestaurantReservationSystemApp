// using System;
// using System.ComponentModel.DataAnnotations;
// using System.ComponentModel.DataAnnotations.Schema;

// namespace RestaurantBackend.Models
// {
//     [Table("Payments")]
//     public class Payment
//     {
//         [Key]
//         [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
//         public int PaymentId { get; set; }

//         public double TotalAmount { get; set; }

//         [Required]
//         public DateTime PaymentDate { get; set; }

//         // Navigation properties
//         public virtual Receipt Receipt { get; set; }

//         public int ReservationId { get; set; }

//         public virtual Reservation Reservation { get; set; }
//     }
// }
