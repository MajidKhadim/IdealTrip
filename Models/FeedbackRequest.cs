namespace IdealTrip.Models
{
	public class FeedbackRequest
	{
		public Guid ServiceId { get; set; }
		public string FeedbackText { get; set; }
		public decimal Rating { get; set; }
	}
}
