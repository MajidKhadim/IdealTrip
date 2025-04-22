using IdealTrip.Helpers;
using System.ComponentModel.DataAnnotations;

namespace IdealTrip.Models.Login
{
	public class ResetPasswordModel
	{
		public string UserId { get; set; }
		public string Token { get; set; }
		[Required]
		[MinLength(6)]
		[StrongPassword]
		[DataType(DataType.Password)]
		public string NewPassword { get; set; }

		[Required]
		[MinLength(6)]
		[Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
		[DataType(DataType.Password)]
		public string ConfirmPassword { get; set; }
	}
}
