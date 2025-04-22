using System.ComponentModel.DataAnnotations;
using IdealTrip.Helpers;

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
        [StrongPassword]
        [MinLength(6)]
        public string Password { get; set; }
		[Required]
		[Compare("Password", ErrorMessage = "Passwords do not match.")]
		[MinLength(6)]
		public string ConfirmPassword {  get; set; }

		[Required]
        [PakistaniPhoneNumber]
		public string PhoneNumber { get; set; }
		[Required]
        public string Address { get; set; }
        public IFormFile? ProfilePhoto { get; set; }
    }
}
