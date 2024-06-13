using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RestaurantBackend.Models;

namespace RestaurantBackend.Data
{
    public class ApplicationDbContext : IdentityDbContext // Inherit from IdentityDbContext
    {
        // DbSets for your entities

        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Customer> Customers { get; set; }

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
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .IsRequired();

            modelBuilder.Entity<Customer>()
            .HasIndex(c => c.Email)
            .IsUnique();


        }
    }
}
