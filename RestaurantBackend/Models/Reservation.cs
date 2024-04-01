using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RestaurantBackend.Validation;

namespace RestaurantBackend.Models
{
    [Table("Reservations")]
    public class Reservation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReservationId { get; set; }

        [Required]
        [CustomValidation(typeof(ReservationValidator), nameof(ReservationValidator.ValidateReservationTime))]
        public DateTime ReservationTime { get; set; }
        
        public DateTime ReservationEndTime { get; set; }

        // [Required]
        // public int TableId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        public int NumberOfGuests { get; set; }

        // Navigation properties
        
        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; }

        // public virtual Table Table { get; set; }

    }
}
