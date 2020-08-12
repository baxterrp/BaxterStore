using System.ComponentModel.DataAnnotations;

namespace BaxterStore.Service.POCOs
{
    public class User
    {
        public string Id { get; set; }

        [Required]
        public string FirstName { get; set; }
        
        [Required]
        public string LastName { get; set; }
        
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
