using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RestaurantBackend.Models;

namespace RestaurantBackend.Data
{
    public class ApplicationDbContext : DbContext
    {
        // DbSets for your entities
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Table> Tables { get; set; } // Assuming Table is the class name for your table entity
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Receipt> Receipts { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // The OnConfiguring method is optional when using AddDbContext in Startup.cs or Program.cs
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Only use if not configuring the connection string in Startup.cs or Program.cs
            // optionsBuilder.UseSqlServer("Server=localhost;Database=MyRestaurantDb;User ID=myUsername;Password=myPassword;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Reservation>()
            .HasOne(r => r.Payment) 
            .WithOne(p => p.Reservation) 
            .HasForeignKey<Payment>(p => p.ReservationId); 

            // Fluent API configurations go here
            // Example: modelBuilder.Entity<Reservation>().HasKey(r => r.ReservationId);
            // Configure relationships, indices, etc.

        }
    }
}
