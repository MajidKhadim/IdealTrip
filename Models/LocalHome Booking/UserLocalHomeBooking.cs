using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace IdealTrip.Models.LocalHome_Booking
{
	public class UserLocalHomeBooking
	{
		[Key]
		public Guid Id { get; set; }  // Unique booking ID

		[Required]
		public Guid UserId { get; set; }  // ID of the user who booked

		[Required]
		public Guid LocalHomeId { get; set; }  // ID of the booked Local Home

		[Required]
		public int TotalDays { get; set; }  // Number of days booked

		[Required]
		[Column(TypeName = "decimal(18,2)")]
		public decimal TotalAmount { get; set; }  // Total cost of booking

		[Required]
		[MaxLength(50)]
		public string Status { get; set; } = "Pending";  // Booking status (Pending, Paid, Cancelled, etc.)

		public string? PaymentIntentId { get; set; }  // Stripe Payment Intent ID

		[Required]
		public DateTime BookingDate { get; set; } = DateTime.Now;  // Date of booking

		// Relationships
		[ForeignKey("UserId")]
		public virtual ApplicationUser User { get; set; }  // Reference to User

		[ForeignKey("LocalHomeId")]
		public virtual LocalHome LocalHome { get; set; }  // Reference to Local Home
	}

}
