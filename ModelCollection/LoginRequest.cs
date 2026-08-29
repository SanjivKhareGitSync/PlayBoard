using System.ComponentModel.DataAnnotations;

namespace PlayBoard
{
    public class LoginRequest
    {
        [Required]
        public required string UserName { get; set; }
        [Required]
        public required string Password { get; set; }
    }
}
