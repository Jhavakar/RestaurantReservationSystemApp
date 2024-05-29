using System;
using System.ComponentModel.DataAnnotations;

namespace RestaurantBackend.ViewModels
{
    public class ReservationVM
    {
        public int? ReservationId { get; set; }  
        public string FirstName { get; set; } = string.Empty;
        
        public string LastName { get; set; } = string.Empty;
        
        public string EmailAddress { get; set; } = string.Empty;
        
        public string? PhoneNumber { get; set; } = string.Empty;
        
        [Required]
        public DateTime ReservationTime { get; set; }
        
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Number of guests must be at least 1.")]
        public int NumberOfGuests { get; set; }
        public bool IsNewAccount { get; set; }
    }
}
