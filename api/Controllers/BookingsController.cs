using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaunaBooking.Api.Data;
using SaunaBooking.Api.Models;

namespace SaunaBooking.Api.Controllers
{
    [ApiController]
    [Route("bookings")]
    public class BookingsController : ControllerBase
    {
        private readonly SaunaBookingDbContext _dbContext;

        public BookingsController(SaunaBookingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET /bookings
        [HttpGet]
        public async Task<ActionResult<List<Booking>>> GetBookings()
        {
            Console.WriteLine(">>> [GET] /bookings endpoint HIT!");

            try
            {
                var results = await _dbContext.Bookings.ToListAsync();
                Console.WriteLine($">>> [GET] Retrieved {results.Count} bookings from DB");
                return results;
            }
            catch (Exception ex)
            {
                Console.WriteLine($">>> [GET] ERROR: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }

        // POST /bookings
        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] Booking booking)
        {
            Console.WriteLine($"Received booking from {booking.Username} on {booking.Date} at {booking.StartTime}");

            bool exists = await _dbContext.Bookings.AnyAsync(b =>
                b.Date.Date == booking.Date.Date &&
                b.StartTime == booking.StartTime);

            if (exists)
            {
                Console.WriteLine("Conflict: Slot already booked.");
                return Conflict("Slot already booked.");
            }

            _dbContext.Bookings.Add(booking);
            await _dbContext.SaveChangesAsync();

            Console.WriteLine("Booking saved.");
            return CreatedAtAction(nameof(GetBookings), new { booking.Date, booking.StartTime }, booking);
        }
    }
}