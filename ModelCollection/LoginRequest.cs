using System.ComponentModel.DataAnnotations;

namespace PlayBoard
{
    public class LoginRequest
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
