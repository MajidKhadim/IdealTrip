using System.ComponentModel.DataAnnotations;

namespace IdealTrip.Models.Login
{
	public class ResetPasswordModel
	{
		[Required]
		[EmailAddress]
			public string Email { get; set; }
		[Required]
			public string Token { get; set; }
		[Required]
			public string NewPassword { get; set; }
		[Required]
			public string ConfirmPassword { get; set; }

	}
}
