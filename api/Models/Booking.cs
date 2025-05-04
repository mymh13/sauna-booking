namespace SaunaBooking.Api.Models
{
    public class Booking
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string Username { get; set; } = string.Empty;

        public string Type { get; set; } = "Private"; // "Private", "Open", "Blocked"

        public DateTime Date { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }
    }
}