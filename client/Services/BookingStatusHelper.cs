namespace SaunaBooking.Client.Services
{
    public static class BookingStatusHelper
    {
        public static string GetColorForStatus(string status) => status switch
        {
            "Free" => "#228B22",     // Forest Green
            "Private" => "#f08080",  // Light red
            "Open" => "#add8e6",     // Light blue
            "Blocked" => "#d3d3d3",  // Light gray
            _ => "white"
        };
    }
}