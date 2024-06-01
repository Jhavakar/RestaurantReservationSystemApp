using System;
using System.ComponentModel.DataAnnotations;

namespace RestaurantBackend.ViewModels
{
    public class ReservationVM
    {
        public int ReservationId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string ReservationDate { get; set; } = string.Empty;
        public string ReservationTime { get; set; } = string.Empty;
        public DateTime ReservationDateTime { get; set; }
        public int NumberOfGuests { get; set; }
        public bool IsNewAccount { get; set; }
    }

}
