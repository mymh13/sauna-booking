using SaunaBooking.Client.Models;
using System.Net.Http.Json;

namespace SaunaBooking.Client.Services
{
    public class UserSessionService
    {
        private readonly HttpClient _http;
        private string? _token;

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
            try
            {
                Console.WriteLine($"Attempting login for user: {request.Username}");
                var response = await _http.PostAsJsonAsync("/api/auth/login", request);
                Console.WriteLine($"Login response status: {response.StatusCode}");
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Login failed. Status: {response.StatusCode}, Content: {errorContent}");
                    return new LoginResponse { Success = false, Message = "Felaktiga uppgifter." };
                }

                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                Console.WriteLine($"Login response: Success={result?.Success}, Role={result?.Role}, HasToken={!string.IsNullOrEmpty(result?.Token)}");
                
                if (result is { Success: true })
                {
                    Username = request.Username;
                    Role = result.Role;
                    _token = result.Token;
                    _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
                    NotifyStateChanged();
                }

                return result ?? new LoginResponse { Success = false, Message = "Okänt fel." };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login exception: {ex}");
                return new LoginResponse { Success = false, Message = $"Ett fel uppstod: {ex.Message}" };
            }
        }

        public void Logout()
        {
            Username = null;
            Role = null;
            _token = null;
            _http.DefaultRequestHeaders.Authorization = null;
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