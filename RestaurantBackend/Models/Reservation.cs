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
        public int Id { get; set; }

        [Required]
        public DateTime ReservationDate { get; set; }


        [Required]
        [CustomValidation(typeof(ReservationValidator), nameof(ReservationValidator.ValidateReservationTime))]
        public DateTime ReservationTime { get; set; }
        public DateTime ReservationEndTime { get; set; } 


        [Required]
        [MaxLength(100)]
        public string Title { get; set; }  = string.Empty;

        public int TableId { get; set; } 
        public double Total { get; set; }

        // Navigation properties
        public virtual Payment Payment { get; set; }
        public virtual Table Table { get; set; }
        public int CustomerId { get; set; }
        public virtual Customer Customer { get; set; } // Ensure this is correctly set up in EF configuration for FK

        public bool IsActive { get; set; } = true;
    }
}
