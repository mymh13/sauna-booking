using Microsoft.EntityFrameworkCore;
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
    }
}