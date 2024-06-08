using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantBackend.Models
{
    [Table("Users")]
    public class Login
    {
        [Key]
        [Required]
        [EmailAddress]
        public string Email { get; set; }  = string.Empty;

        [DataType(DataType.Password)]
        public string Password { get; set; }  = string.Empty;
    }
}
