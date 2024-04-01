// using System.ComponentModel.DataAnnotations;
// using System.ComponentModel.DataAnnotations.Schema;

// namespace RestaurantBackend.Models
// {
//     [Table("Receipts")]
//     public class Receipt
//     {
//         [Key]
//         [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
//         public int ReceiptId { get; set; }

//         public double TotalPrice { get; set; }

//         // Assuming each receipt is tied to exactly one payment,
//         // and each payment generates exactly one receipt.
//         [Required]
//         public int PaymentId { get; set; }

//         // Assuming each receipt is for a specific reservation.
//         [Required]
//         public int ReservationId { get; set; }

//         // Navigation properties
//         [ForeignKey("PaymentId")]
//         public virtual Payment Payment { get; set; }

//         [ForeignKey("ReservationId")]
//         public virtual Reservation Reservation { get; set; }

//     }
// }
