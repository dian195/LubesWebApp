using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class SignInDTO
    {
        [Required]
        public string usernameOrEmail { get; set; }
        [Required, DataType(DataType.Password)]
        public string password { get; set; }
        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }
    }
}
