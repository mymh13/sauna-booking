using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaunaBooking.Api.Models;
using SaunaBooking.Api.Data;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using SaunaBooking.Api.Models.DTOs;
using System.Security.Claims;

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

        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("users")]
        public async Task<ActionResult<List<UserDto>>> GetUsers()
        {
            var users = await _dbContext.Users
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Role = u.Role
                })
                .ToListAsync();

            return Ok(users);
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpPost("users")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            if (await _dbContext.Users.AnyAsync(u => u.Username == request.Username))
            {
                return BadRequest(new { Message = "Username already exists" });
            }

            var user = new User
            {
                Username = request.Username,
                PasswordHash = HashPassword(request.Password),
                Role = request.Role
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            return Ok(new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Role = user.Role
            });
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
        {
            var user = await _dbContext.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { Message = "User not found" });
            }

            if (request.Password != null)
            {
                user.PasswordHash = HashPassword(request.Password);
            }

            if (request.Role != null)
            {
                user.Role = request.Role;
            }

            await _dbContext.SaveChangesAsync();

            return Ok(new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Role = user.Role
            });
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _dbContext.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { Message = "User not found" });
            }

            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync();

            return Ok(new { Message = "User deleted successfully" });
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes).ToLower();
        }
    }
}