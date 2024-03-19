using System;
using System.ComponentModel.DataAnnotations;

namespace RestaurantBackend.Validation
{
    public class ReservationValidator
    {
        // Assuming the restaurant operates from 12 PM to 11 PM
        public static ValidationResult ValidateReservationTime(DateTime reservationTime, ValidationContext validationContext)
        {
            if (reservationTime.TimeOfDay < new TimeSpan(12, 0, 0) || reservationTime.TimeOfDay > new TimeSpan(23, 0, 0))
            {
                return new ValidationResult("Reservation time must be between 12 PM and 11 PM.");
            }
            return ValidationResult.Success;
        }
    }
}
