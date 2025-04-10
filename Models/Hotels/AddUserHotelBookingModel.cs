using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace IdealTrip.Models.Hotels
{
	public class AddUserHotelBookingModel
	{
		[Required]
		public string HotelRoomId { get; set; } 
		[Required]
		public int TotalDays { get; set; }    
		[Required]
		public DateOnly StartDate { get; set; }
		[Required]
		public DateOnly EndDate { get; set; }
	}
}
