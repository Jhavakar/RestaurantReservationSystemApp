using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantBackend.Models
{
    public class Customer : IdentityUser
    {
        [Required, MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required, MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    }
}
