using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RestaurantBackend.ViewModels {
    public class TableVM
    {
        public int TableId { get; set; }

        [Required, MaxLength(100)]
        public string? TableNumber { get; set; }

        [Required, Range(1, int.MaxValue)]
        public int Capacity { get; set; }

        public bool IsAvailable { get; set; } // Consider dynamically calculating this based on reservations

        [MaxLength(255)]
        public string? LocationDescription { get; set; } // Optional: can be null if not set

        public string? ImageUrl { get; set; } // URL to an image of the table or its location

        // Features could be represented as a list of strings or predefined enums for known features
        public List<string> Features { get; set; } = new List<string>();
    }
}
