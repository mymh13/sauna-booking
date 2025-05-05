using SaunaBooking.Client.Models;
using System.Net.Http.Json;

namespace SaunaBooking.Client.Services
{
    public class UserSessionService
    {
        private readonly HttpClient _http;

        public string? Username { get; private set; }
        public string? Role { get; private set; }
        public bool IsLoggedIn => !string.IsNullOrWhiteSpace(Username);
        public bool IsDarkMode { get; private set; } = false;
        public bool ShowMobileMenu { get; private set; } = false;
        public bool IsMobileLayout { get; private set; } = false;

        public string ThemeLabel => IsDarkMode ? "Ljust läge" : "Mörkt läge";
        public string Theme => IsDarkMode ? "dark" : "light";

        public event Action? OnChange;

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
                NotifyStateChanged();
            }

            return result ?? new LoginResponse { Success = false, Message = "Okänt fel." };
        }

        public void Logout()
        {
            Username = null;
            Role = null;
            NotifyStateChanged();
        }

        public void ToggleTheme()
        {
            IsDarkMode = !IsDarkMode;
            NotifyStateChanged();
        }

        public void ToggleMobileMenu()
        {
            ShowMobileMenu = !ShowMobileMenu;
            NotifyStateChanged();
        }

        public void SetMobileLayout(bool isMobile)
        {
            IsMobileLayout = isMobile;
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}