using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;

namespace IdealTrip.Models.Register
{
    public class RegisterTransportorModel : RegisterDtoBase
    {
		[Required]
		public string VehicleDetails { get; set; }
		[Required]
		public IFormFile VehicleRegistrationDoc {  get; set; }
		[Required]
		public IFormFile DriverLicense { get ; set; }
	}
}
