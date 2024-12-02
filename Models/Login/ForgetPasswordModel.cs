using System.ComponentModel.DataAnnotations;

namespace IdealTrip.Models.Login
{
	public class ForgetPasswordModel
	{
		[Required]
		[EmailAddress]
		public string EmailAddress { get; set; }
	}
}
