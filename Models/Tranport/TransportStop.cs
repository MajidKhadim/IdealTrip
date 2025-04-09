using System.ComponentModel.DataAnnotations.Schema;

namespace IdealTrip.Models.Tranport_Booking
{
	public class TransportStop
	{
		public Guid Id { get; set; }
		public Guid TransportId { get; set; }
		public string StopName { get; set; }
		public int Sequence { get; set; } // Order of stops (1 = Starting point)
		public decimal FareFromStart { get; set; } // Fare from the starting point

		[ForeignKey("TransportId")]
		public virtual Transport Transport { get; set; }
	}

}
