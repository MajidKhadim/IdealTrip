using System.ComponentModel.DataAnnotations;

namespace IdealTrip.Models.TourGuide_Booking
{
		public class TourGuideBookingModel
		{
			[Required]
			public Guid TourGuideId { get; set; }  // Tour Guide ID

			[Required]
			public DateTime StartDate { get; set; }  // Tour Start Date

			[Required]
			public DateTime EndDate { get; set; }  // Tour End Date

			[Required]
			[Range(1, int.MaxValue, ErrorMessage = "Number of travelers must be at least 1.")]
			public int NumberOfTravelers { get; set; }  // Number of travelers in the booking

			public string? SpecialRequest { get; set; }  // Any special requests from the user

			public int TotalDays => (EndDate - StartDate).Days + 1;  // Automatically calculated total days

		}
}
