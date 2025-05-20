namespace IdealTrip.Models
{
	public class UserBookingSummaryDto
	{
		public Guid BookingId { get; set; }
		public string BookingType { get; set; } 
		public string ServiceName { get; set; } 
		public string? Location { get; set; }
		public DateTime BookingDate { get; set; }
		public int? NumberOfPeople { get; set; }
		public decimal AmountPaid { get; set; }
		public string Status { get; set; }
		public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
    }

}
