using System;
using System.ComponentModel.DataAnnotations;

namespace RestaurantBackend.ViewModels
{
    public class ReservationViewModel
    {
        public int? ReservationId { get; set; }

        [Required]
        public DateTime ReservationTime { get; set; }

        [Required]
        public int TableId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "The number of guests must be at least 1.")]
        public int NumberOfGuests { get; set; }
    }
}
