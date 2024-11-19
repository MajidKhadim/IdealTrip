using System.ComponentModel.DataAnnotations;

namespace IdealTrip.Models.Register
{
    public class RegisterDtoBase
    {
        [Required]
        public string FullName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [MinLength(6)]
        public string Password { get; set; }
        public string Address { get; set; }
        public IFormFile ProfilePhoto { get; set; }
    }
}
