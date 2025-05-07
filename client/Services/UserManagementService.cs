using System.Net.Http.Json;
using SaunaBooking.Client.Models.DTOs;

namespace SaunaBooking.Client.Services
{
    public class UserManagementService
    {
        private readonly HttpClient _http;

        public UserManagementService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<UserDto>> GetUsersAsync()
        {
            var response = await _http.GetFromJsonAsync<List<UserDto>>("/api/auth/users");
            return response ?? new List<UserDto>();
        }

        public async Task<UserDto> CreateUserAsync(CreateUserRequest request)
        {
            var response = await _http.PostAsJsonAsync("/api/auth/users", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<UserDto>() ?? throw new Exception("Failed to create user");
        }

        public async Task<UserDto> UpdateUserAsync(int id, UpdateUserRequest request)
        {
            var response = await _http.PutAsJsonAsync($"/api/auth/users/{id}", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<UserDto>() ?? throw new Exception("Failed to update user");
        }

        public async Task DeleteUserAsync(int id)
        {
            var response = await _http.DeleteAsync($"/api/auth/users/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
} 