using System.ComponentModel.DataAnnotations;

namespace PlayBoard.ModelCollection
{
    public class RegistrationForm
    {
        [Required]
        public required string UserName { get; set; }
        [Required]
        public required string Password { get; set; }

    }
}