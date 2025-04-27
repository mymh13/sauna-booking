using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaunaBooking.Api.Models;
using SaunaBooking.Api.Data;
using System.Security.Cryptography;
using System.Text;

namespace SaunaBooking.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly SaunaBookingDbContext _dbContext;

        public AuthController(SaunaBookingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null)
            {
                return Unauthorized(new LoginResponse { Success = false, Message = "Invalid credentials" });
            }

            string hashedPassword = HashPassword(request.Password);

            if (user.PasswordHash != hashedPassword)
            {
                return Unauthorized(new LoginResponse { Success = false, Message = "Invalid credentials" });
            }

            return Ok(new LoginResponse
            {
                Success = true,
                Message = "Login successful",
                Role = user.Role
            });
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes).ToLower();
        }
    }
}