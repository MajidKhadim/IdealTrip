using System.ComponentModel.DataAnnotations;

namespace IdealTrip.Models.Login
{
	public class ResetPasswordModel
	{
		public string UserId { get; set; }
		public string Token { get; set; }
		[Required]
		[DataType(DataType.Password)]
		public string NewPassword { get; set; }

		[Required]
		[DataType(DataType.Password)]
		public string ConfirmPassword { get; set; }
	}
}
