using System;
using System.ComponentModel.DataAnnotations;

namespace RestaurantBackend.ViewModels
{
    public class ReservationVM
    {
        public int? ReservationId { get; set; }  
        [Required]
        public DateTime ReservationTime { get; set; }  // Includes both date and time

        [Required, EmailAddress]
        public string EmailAddress { get; set; }  = string.Empty;

        [Required, Range(1, int.MaxValue, ErrorMessage = "The number of guests must be at least 1.")]
        public int NumberOfGuests { get; set; }
    }
}
 