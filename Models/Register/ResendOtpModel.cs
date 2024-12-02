using System.ComponentModel.DataAnnotations;

namespace IdealTrip.Models.Register
{
	public class ResendOtpModel
	{
		[Required]
		[EmailAddress]
		public string email { get; set; }
	}
}
