using System.ComponentModel.DataAnnotations.Schema;

namespace IdealTrip.Models.Tranport_Booking
{
	public class UserTransportBooking
	{
		public Guid Id { get; set; }
		public Guid UserId { get; set; }
		public Guid TransportId { get; set; }
		public int SeatsBooked { get; set; }
		public decimal TicketPrice { get; set; } // Price per seat
		public decimal TotalFare { get; set; } // SeatsBooked * (EndStop.FareFromStart - StartStop.FareFromStart)
		public string? PaymentIntentId { get; set; }
		public string Status { get; set; } // "Pending", "Paid", "Cancelled"
		public DateTime BookingDate { get; set; }
		public DateTime CreatedAt { get; set; }

		[ForeignKey("UserId")]
		public virtual ApplicationUser User { get; set; }

		[ForeignKey("TransportId")]
		public virtual Transport Transport { get; set; }
	}

}