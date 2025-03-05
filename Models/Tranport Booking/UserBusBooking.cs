using System.ComponentModel.DataAnnotations.Schema;

namespace IdealTrip.Models.Tranport_Booking
{
	public class UserBusBooking
	{
		public Guid Id { get; set; }
		public Guid UserId { get; set; }
		public Guid TransportId { get; set; }
		public string BookingType { get; set; } // "BusSeat"
		public int SeatsBooked { get; set; }
		public decimal TicketPrice { get; set; } // Price per seat, fetched from Transport
		public decimal TotalFare { get; set; } // Total fare (SeatsBooked * TicketPrice)
		public string PaymentIntentId { get; set; }
		public string Status { get; set; } // "Pending", "Paid", "Cancelled"
		public DateTime BookingDate { get; set; }
		public DateTime CreatedAt { get; set; }
		[ForeignKey("UserId")]

		public virtual ApplicationUser User { get; set; } // Navigation Property
		[ForeignKey("TransportId")]
		public virtual Transport Transport { get; set; }

	}

}