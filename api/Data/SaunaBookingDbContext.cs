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
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Booking>()
                .HasIndex(b => new { b.Date, b.StartTime })
                .IsUnique();
        }
    }
}