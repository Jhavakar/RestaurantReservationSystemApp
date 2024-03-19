using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantBackend.Models
{
    [Table("RestaurantTables")]
    public class Table
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Auto-generate TableId
        public int TableId { get; set; }

        [Required]
        [MaxLength(100)] // Example length, adjust as needed
        public string? TableNumber { get; set; } // Or could be a string if you use names/codes

        [Required]
        public int Capacity { get; set; } // Number of people that can be seated at the table

        public bool IsAvailable { get; set; } // To indicate if the table is currently available for booking

        [Required]
        public string? LocationDescription { get; set; } // Description of the table's location within the restaurant

        // Navigation properties
        public virtual ICollection<Reservation> Reservations { get; set; }
    }

}
