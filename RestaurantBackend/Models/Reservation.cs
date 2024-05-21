using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using RestaurantBackend.Validation;

namespace RestaurantBackend.Models
{
    [Table("Reservations")]
    public class Reservation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReservationId { get; set; }

        [Required, ReservationTimeValidation]
        public DateTime ReservationTime { get; set; } 

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int NumberOfGuests { get; set; }

        // Navigation property
        [ForeignKey("UserId")]
        public virtual Customer User { get; set; } 
    }
}
