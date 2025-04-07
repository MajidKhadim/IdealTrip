namespace IdealTrip.Models.Database_Tables
{
	public class EmailBounce
	{
		public int Id { get; set; }
		public string Email { get; set; }
		public string Reason { get; set; }
		public DateTime BouncedAt { get; set; }
	}

}
