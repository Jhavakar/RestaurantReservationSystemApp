using System.ComponentModel.DataAnnotations;

namespace RestaurantBackend.ViewModels
{
    public class ResetPasswordVM
    {
        public string Email { get; set; } // Optional: if you want to display or verify the email in the form.

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string Token { get; set; }
    }

}
