using System.Net.Http.Json;
using client.Models;

namespace client.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;

        public AuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/auth/login", request);

            if (!response.IsSuccessStatusCode)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "Kunde inte logga in. Kontrollera användarnamn och lösenord."
                };
            }

            var loginResult = await response.Content.ReadFromJsonAsync<LoginResponse>();
            return loginResult ?? new LoginResponse { Success = false, Message = "Okänt fel." };
        }
    }
}