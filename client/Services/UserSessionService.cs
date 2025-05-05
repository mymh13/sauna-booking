using SaunaBooking.Client.Models;
using System.Net.Http.Json;

namespace SaunaBooking.Client.Services
{
    public class UserSessionService
    {
        private readonly HttpClient _http;
        public string? Username { get; set; }
        public string? Role { get; set; }
        public bool IsDarkMode { get; set; } = false;

        public string ThemeLabel => IsDarkMode ? "Ljust läge" : "Mörkt läge";
        public string Theme => IsDarkMode ? "dark" : "light";

        public UserSessionService(HttpClient http)
        {
            _http = http;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            var response = await _http.PostAsJsonAsync("/api/auth/login", request);
            if (!response.IsSuccessStatusCode)
                return new LoginResponse { Success = false, Message = "Felaktiga uppgifter." };

            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (result is { Success: true })
            {
                Username = request.Username;
                Role = result.Role;
            }

            return result ?? new LoginResponse { Success = false, Message = "Okänt fel." };
        }

        public void ToggleTheme()
        {
            IsDarkMode = !IsDarkMode;
        }

        public void Logout()
        {
            Username = null;
            Role = null;
        }

        public bool ShowMobileMenu { get; private set; } = false;

        public void ToggleMobileMenu()
        {
            ShowMobileMenu = !ShowMobileMenu;
        }
    }
}