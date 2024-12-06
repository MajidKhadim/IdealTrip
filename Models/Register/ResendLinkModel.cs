using System.ComponentModel.DataAnnotations;

namespace IdealTrip.Models.Register
{
	public class ResendLinkModel
	{
		[Required]
		[EmailAddress]
		public string email { get; set; }
	}
}
