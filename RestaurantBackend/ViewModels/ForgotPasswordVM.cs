using System.ComponentModel.DataAnnotations;

namespace RestaurantBackend.ViewModels
{
    public class ForgotPasswordVM
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

}
