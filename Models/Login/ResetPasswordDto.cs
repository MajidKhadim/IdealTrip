using System.ComponentModel.DataAnnotations;

namespace IdealTrip.Models.Login
{
	public class ResetPasswordDto
	{
		[Required]
		public string Token { get; set; }
		[Required]
		[EmailAddress]
		public string Email { get; set; }
		[Required]
		[MinLength(6)]
		public string NewPassword { get; set; }
	}
}
