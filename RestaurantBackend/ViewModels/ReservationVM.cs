using System;
using System.ComponentModel.DataAnnotations;

namespace RestaurantBackend.ViewModels
{
    public class ReservationVM
    {
        // Optional for creating a new reservation, useful for updates
        public int? ReservationId { get; set; }

        [Required]
        public DateTime ReservationTime { get; set; } // Includes both date and time

        [Required, MaxLength(50)]
        public string FirstName { get; set; }

        [Required, MaxLength(50)]
        public string LastName { get; set; }

        [Required, EmailAddress]
        public string EmailAddress { get; set; }

        [Required]
        public int TableId { get; set; }

        [Required]
        public int? CustomerId { get; set; }

        [Required, Range(1, int.MaxValue, ErrorMessage = "The number of guests must be at least 1.")]
        public int NumberOfGuests { get; set; }
    }
}
