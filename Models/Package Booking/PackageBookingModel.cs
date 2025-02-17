using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace IdealTrip.Models.Package_Booking
{
	public class PackageBookingModel
	{
		[Required]
		public string FullName { get; set; }
		[Required]
		public string Email { get; set; }
		[Required]
		public string PhoneNumber { get; set; }
		[Required]
		public int NumberOfTravelers { get; set; }
		[Required]
		public Guid PackageId { get; set; }
		public decimal TotalBill { get; set; }
	}
}
