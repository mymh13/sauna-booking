namespace SaunaBooking.Client.Services
{
    public class UserSessionService
    {
        public string? Username { get; set; }
        public bool IsDarkMode { get; set; } = false;

        public string ThemeLabel => IsDarkMode ? "Ljust läge" : "Mörkt läge";

        public void ToggleTheme()
        {
            IsDarkMode = !IsDarkMode;
        }
    }
}