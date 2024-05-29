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

        [Required]
        public DateTime ReservationTime { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Number of guests must be at least 1")]
        public int NumberOfGuests { get; set; }

        [Required]
        [ForeignKey("User")]
        public string UserId { get; set; } = string.Empty;
        public virtual Customer User { get; set; }
    }
}
