using System;
using System.ComponentModel.DataAnnotations;

namespace RestaurantBackend.ViewModels {
    public class ReservationVM
    {
        public int ReservationId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        public int TableId { get; set; }

        [Required]
        public DateTime ReservationTime { get; set; }

        [Required, Range(1, int.MaxValue)]
        public int NumberOfGuests { get; set; }
    }
}
