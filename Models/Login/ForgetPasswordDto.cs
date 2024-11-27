using System.ComponentModel.DataAnnotations;

namespace IdealTrip.Models.Login
{
	public class ForgetPasswordDto
	{
		[Required]
		[EmailAddress]
		public string EmailAddress { get; set; }
	}
}
