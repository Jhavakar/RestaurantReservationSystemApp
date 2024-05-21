using System;
using System.ComponentModel.DataAnnotations;

namespace RestaurantBackend.Validation
{
    public class ReservationTimeValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is DateTime reservationTime)
            {
                // Validate the reservation time between 12 PM and 11 PM
                TimeSpan openingTime = new TimeSpan(12, 0, 0); // 12:00 PM
                TimeSpan closingTime = new TimeSpan(23, 0, 0); // 11:00 PM

                if (reservationTime.TimeOfDay < openingTime || reservationTime.TimeOfDay > closingTime)
                {
                    return new ValidationResult("Reservation time must be between 12 PM and 11 PM.");
                }

                return ValidationResult.Success;
            }

            return new ValidationResult("Invalid reservation time format.");
        }
    }
}
