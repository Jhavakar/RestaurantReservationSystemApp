using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RestaurantBackend.ViewModels
{
    public class LoginVM
    {
        [Required, EmailAddress]
        public string EmailAddress { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
