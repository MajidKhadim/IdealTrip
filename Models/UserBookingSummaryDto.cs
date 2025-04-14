namespace IdealTrip.Models
{
	public class UserBookingSummaryDto
	{
		public Guid BookingId { get; set; }
		public string BookingType { get; set; } // "Hotel", "LocalHome", "Transport", "TourGuide"
		public string ServiceName { get; set; } // Hotel name, Local Home name, etc.
		public string? Location { get; set; }
		public DateTime BookingDate { get; set; }
		public int? NumberOfPeople { get; set; }
		public decimal AmountPaid { get; set; }
		public string Status { get; set; }
	}

}
