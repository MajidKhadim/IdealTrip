using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace IdealTrip.Models.Package_Booking
{
	public class PackageBookingModel
	{
		[Required]
		public int NumberOfTravelers { get; set; }
		[Required]
		public Guid PackageId { get; set; }
	}
}
