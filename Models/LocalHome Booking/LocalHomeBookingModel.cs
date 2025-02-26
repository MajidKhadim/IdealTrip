using System.ComponentModel.DataAnnotations;

namespace IdealTrip.Models.LocalHome_Booking
{
	public class LocalHomeBookingModel
	{
		[Required]
		public Guid LocalHomeId { get; set; }  // ID of the Local Home being booked
		[Required]
		public int TotalDays { get; set; }     // Number of days booked
	}
}
