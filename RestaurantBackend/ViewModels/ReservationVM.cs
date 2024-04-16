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
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string EmailAddress { get; set; } = string.Empty;

        // public int TableId { get; set; }

        public int? CustomerId { get; set; }

        [Required, Range(1, int.MaxValue, ErrorMessage = "The number of guests must be at least 1.")]
        public int NumberOfGuests { get; set; }
    }
}
