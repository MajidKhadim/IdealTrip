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
		[Required]
		[MinLength(6)]
		public string ConfirmPassword {  get; set; }

        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        public string Address { get; set; }
        public IFormFile? ProfilePhoto { get; set; }
    }
}
