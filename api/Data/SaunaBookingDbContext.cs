using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SaunaBooking.Api.Models;

namespace SaunaBooking.Api.Data
{
    public class SaunaBookingDbContext : DbContext
    {
        public SaunaBookingDbContext(DbContextOptions<SaunaBookingDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Prevent DateTimeKind=Utc issues on Booking.Date
            var dateConverter = new ValueConverter<DateTime, DateTime>(
                v => DateTime.SpecifyKind(v, DateTimeKind.Unspecified), // Save as unspecified
                v => DateTime.SpecifyKind(v, DateTimeKind.Unspecified)  // Load as unspecified
            );

            modelBuilder.Entity<Booking>()
                .Property(b => b.Date)
                .HasConversion(dateConverter);
        }
    }
}