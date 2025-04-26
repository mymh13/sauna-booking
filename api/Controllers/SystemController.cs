using Microsoft.AspNetCore.Mvc;
using SaunaBooking.Api.Models;

namespace SaunaBooking.Api.Controllers
{
[ApiController]
    [Route("[controller]")]
    public class SystemController : ControllerBase
    {
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            var response = new PingResponse
            {
                Message = "Pong",
                Timestamp = DateTime.UtcNow
            };

            return Ok(response);
        }
    }
}