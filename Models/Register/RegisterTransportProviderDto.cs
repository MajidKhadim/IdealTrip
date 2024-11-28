using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;

namespace IdealTrip.Models.Register
{
    public class RegisterTransportProviderDto : RegisterWithProofDto
    {
		[Required]
		public string VehicleDetails { get; set; }
		[Required]
		public IFormFile VehicleRegistrationForm {  get; set; }
		[Required]
		public IFormFile DriverLicense { get ; set; }
	}
}
