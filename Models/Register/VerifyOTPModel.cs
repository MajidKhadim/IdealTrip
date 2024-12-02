using System.ComponentModel.DataAnnotations;

namespace IdealTrip.Models.Register
{
	public class VerifyOTPModel
	{
		[Required]
		[EmailAddress]
		public string email { get; set; }
		[Required]
		public string Otp {  get; set; }
	}
}
