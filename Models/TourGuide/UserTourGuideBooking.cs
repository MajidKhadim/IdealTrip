using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace IdealTrip.Models.TourGuide_Booking
{
	public class UserTourGuideBooking
	{
		[Key]
		public Guid Id { get; set; }  // Primary Key

		[Required]
		public Guid UserId { get; set; }  // Foreign Key (Tourist)

		[Required]
		public Guid TourGuideId { get; set; }  // Foreign Key (Tour Guide)

		[Required]
		public DateTime BookingDate { get; set; } // When the booking was made

		[Required]
		public DateTime StartDate { get; set; }  // Tour Start Date

		[Required]
		public DateTime EndDate { get; set; }  // Tour End Date

		[Required]
		public decimal TotalAmount { get; set; }  // Total Price for the booking

		[Required]
		public int NumberOfTravelers { get; set; }  // Number of people in the booking

		public string? SpecialRequest { get; set; }  // Any special request from the user

		[Required]
		public string Status { get; set; } = "Pending";  // Status: Pending, Approved, Cancelled

		// Navigation Properties
		[ForeignKey("UserId")]
		public virtual ApplicationUser User { get; set; }
		[ForeignKey("TourGuideId")]
		public virtual TourGuide TourGuide { get; set; }
        public string? PaymentIntentId { get; set; }
		[Required]
        public int TotalDays { get; set; }
    }
}
