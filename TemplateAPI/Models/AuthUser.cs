using System.ComponentModel.DataAnnotations;

namespace TemplateAPI.Models
{
    public class AuthUser
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
