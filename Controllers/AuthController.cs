using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PlayBoard.ClassCollection;
using PlayBoard.ModelCollection;
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
        public AuthController(IConfiguration config)
        {
            _config = config;
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {

            // Dummy authentication logic
            Auth auth = new Auth();
            if (!auth.VerifyUserData(request))
            {
                return Unauthorized();
            }
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, request.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, request.UserName),
                new Claim(ClaimTypes.Role, "Admin") // example role
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
        public IActionResult Registration(RegistrationForm registrationForm)
        {
            Auth auth = new Auth();
            string registrationResult = auth.RegisterUser(registrationForm);
            if (registrationResult == "1")
            { 
                return Ok("SUCCESS");
            }
            else
            {
                return Ok("FAILED");
            }
        }
    }
}