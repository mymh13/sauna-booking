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

            booking.Date = booking.Date.Date;
            booking.StartTime = TimeSpan.FromHours(booking.StartTime.Hours); // Snap to full hour

            bool exists = await _dbContext.Bookings.AnyAsync(b =>
                b.Date.Date == booking.Date.Date &&
                b.StartTime == booking.StartTime);

            if (exists)
            {
                Console.WriteLine("Conflict: Slot already booked.");
                return Conflict("Slot already booked.");
            }

            // May need to get the user's role from the JWT claims
            var isAdmin = User.IsInRole("admin");
            if (booking.Type == "Blocked" && !isAdmin)
            {
                return Forbid("Only admins can book 'Blockerad' slots.");
            }

            _dbContext.Bookings.Add(booking);
            await _dbContext.SaveChangesAsync();

            Console.WriteLine("Booking saved.");
            return CreatedAtAction(nameof(GetBookings), new { booking.Date, booking.StartTime }, booking);
        }

        // DELETE /bookings/{date}/{startTime}
        [HttpDelete("{date}/{startTime}")]
        public async Task<IActionResult> Delete(DateTime date, TimeSpan startTime)
        {
            Console.WriteLine($">>> [DELETE] Requested date: {date:yyyy-MM-dd} time: {startTime}");

            var booking = await _dbContext.Bookings
                .FirstOrDefaultAsync(b => b.Date == date && b.StartTime == startTime);

            if (booking == null)
            {
                Console.WriteLine($">>> [DELETE] No match found.");
                return NotFound();
            }

            Console.WriteLine($">>> [DELETE] Found booking with ID {booking.Id}, deleting.");
            _dbContext.Bookings.Remove(booking);
            await _dbContext.SaveChangesAsync();
            return NoContent();
        }
    }
}