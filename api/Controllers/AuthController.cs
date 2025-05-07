using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaunaBooking.Api.Models;
using SaunaBooking.Api.Data;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SaunaBooking.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly SaunaBookingDbContext _dbContext;
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _configuration;

        public AuthController(
            SaunaBookingDbContext dbContext, 
            ILogger<AuthController> logger,
            IConfiguration configuration)
        {
            _dbContext = dbContext;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var user = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.Username == request.Username);

                if (user == null)
                {
                    _logger.LogWarning("Login attempt failed: User not found - {Username}", request.Username);
                    return Unauthorized(new LoginResponse { Success = false, Message = "Invalid credentials" });
                }

                string hashedPassword = HashPassword(request.Password);

                if (user.PasswordHash != hashedPassword)
                {
                    _logger.LogWarning("Login attempt failed: Invalid password for user - {Username}", request.Username);
                    return Unauthorized(new LoginResponse { Success = false, Message = "Invalid credentials" });
                }

                var token = GenerateJwtToken(user);

                _logger.LogInformation("User logged in successfully: {Username}", user.Username);
                return Ok(new LoginResponse
                {
                    Success = true,
                    Message = "Login successful",
                    Role = user.Role,
                    Token = token
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user: {Username}", request.Username);
                return StatusCode(500, new LoginResponse { Success = false, Message = "An error occurred during login" });
            }
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("users")]
        public async Task<ActionResult<List<UserDto>>> GetUsers()
        {
            try
            {
                _logger.LogInformation("Fetching users list by admin: {Username}", User.Identity?.Name);
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching users list");
                return StatusCode(500, "An error occurred while fetching users");
            }
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpPost("users")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user: {Username}", request.Username);
                return StatusCode(500, "An error occurred while creating the user");
            }
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user with ID: {UserId}", id);
                return StatusCode(500, "An error occurred while updating the user");
            }
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user with ID: {UserId}", id);
                return StatusCode(500, "An error occurred while deleting the user");
            }
        }

        private string GenerateJwtToken(User user)
        {
            var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not found in configuration");
            var jwtIssuer = _configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer not found in configuration");
            var jwtAudience = _configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience not found in configuration");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes).ToLower();
        }
    }
}