using Microsoft.AspNetCore.Routing.Constraints;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdealTrip.Models.Tranport_Booking
{
	public class Transport
	{
		public Guid Id { get; set; }
		public Guid OwnerId { get; set; }
		public string Name { get; set; }
		public string Type { get; set; } // "Private" or "Bus"
		public int Capacity { get; set; }
		public int SeatsAvailable { get; set; }
		public string StartLocation { get; set; }
		public string Destination { get ; set; }
		public DateTime DepartureTime { get; set; }
		public decimal TicketPrice { get; set; } // Price per seat
		public bool IsAvailable { get; set; }
		public DateTime CreatedAt { get; set; }
		public float Rating {  get; set; }
		public DateTime UpdatedAt { get; set; }

		[ForeignKey("OwnerId")]

		public virtual ApplicationUser Owner { get; set; } // Navigation Property

        public bool IsDeleted { get; set; } = false;

    }


}
