using System.ComponentModel.DataAnnotations;

namespace RestaurantBackend.ViewModels
{
    public class SetPasswordVM
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string EmailAddress { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
