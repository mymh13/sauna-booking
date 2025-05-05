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
            return await _dbContext.Bookings.ToListAsync();
        }

        // POST /bookings
        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] Booking booking)
        {
            // Simple check for duplicates (you can expand this)
            bool exists = await _dbContext.Bookings.AnyAsync(b =>
                b.Date.Date == booking.Date.Date &&
                b.StartTime == booking.StartTime);

            if (exists)
            {
                return Conflict("Slot already booked.");
            }

            _dbContext.Bookings.Add(booking);
            await _dbContext.SaveChangesAsync();
            return CreatedAtAction(nameof(GetBookings), new { booking.Date, booking.StartTime }, booking);
        }
    }
}