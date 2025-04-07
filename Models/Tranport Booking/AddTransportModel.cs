namespace IdealTrip.Models.Tranport_Booking
{
	public class AddTransportModel
	{
		public string Name { get; set; }
		public string Type { get; set; } // "Private" or "Bus"
		public int Capacity { get; set; }
		public int SeatsAvailable { get; set; }
		public string StartLocation { get; set; }
		public string Destination { get; set; }
		public DateTime DepartureTime { get; set; }
		public decimal PricePerSeat { get; set; } // Price per seat

		public IFormFile PrimaryImage { get; set; }

		public ICollection<IFormFile> Images { get; set; }

	}
}
