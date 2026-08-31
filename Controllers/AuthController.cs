using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PlayBoard.ModelCollection;
using PlayBoard.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PlayBoard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IAuthService _authService;

        public AuthController(IConfiguration config, IAuthService authService)
        {
            _config = config;
            _authService = authService;
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!await _authService.VerifyCredentialsAsync(request))
                return Unauthorized();

            var adminUsers = _config.GetSection("AdminUsers").Get<string[]>() ?? Array.Empty<string>();
            bool isAdmin = adminUsers.Contains(request.UserName, StringComparer.OrdinalIgnoreCase);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, request.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, request.UserName),
                new Claim(ClaimTypes.Role, isAdmin ? "Admin" : "User")
            };
            var jwtSettings = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? ""));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiryMinutes"] ?? "60"));
            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return Ok(new { token = tokenString });
        }
        [AllowAnonymous]
        [HttpPost("Register")]
        public async Task<IActionResult> Registration(RegistrationForm registrationForm)
        {
            var result = await _authService.RegisterUserAsync(registrationForm);
            return result switch
            {
                RegistrationResult.Success => Ok("SUCCESS"),
                RegistrationResult.UserAlreadyExists => Conflict("User already exists"),
                RegistrationResult.InvalidInput => BadRequest("Username and password are required"),
                _ => StatusCode(500, "Something went wrong")
            };
        }
        [AllowAnonymous]
        [HttpGet("Test")]
        public IActionResult Test()
        {
            return Ok("Test successful");
        }
    }
}