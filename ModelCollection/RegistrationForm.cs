using System.ComponentModel.DataAnnotations;

namespace PlayBoard.ModelCollection
{
    public class RegistrationForm
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }

    }
}