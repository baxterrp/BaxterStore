using System.ComponentModel.DataAnnotations;

namespace BaxterStore.Api.Contracts
{
    public class LoginContract
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
