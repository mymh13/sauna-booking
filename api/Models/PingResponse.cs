namespace SaunaBooking.Api.Models
{
    public class PingResponse
    {
        public string Message { get; set; } = "Pong";
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}