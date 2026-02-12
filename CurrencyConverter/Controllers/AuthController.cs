using CurrencyConverter.Models;
using CurrencyConverter.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyConverter.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly JwtService _jwtService;
        private static readonly Dictionary<string, (string Password, string ClientId)> _users
        = new()
                {
        { "admin", ("123", "1001") },
        { "role1", ("password123", "1002") },
        { "role2", ("xyz123", "1003") }
            };  
        public AuthController(JwtService jwtService)
        {
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (!_users.TryGetValue(request.Username, out var user))
                return Unauthorized("Invalid username");

            if (user.Password != request.Password)
                return Unauthorized("Invalid password");
            
            var token = _jwtService.GenerateToken(user.ClientId, request.Username);

            return Ok(new
            {
                access_token = token,
                expires_in = 3600
            });
        }
    }
}
