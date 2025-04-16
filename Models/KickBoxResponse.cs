namespace IdealTrip.Models
{
	public class KickboxResponse
	{
		public string result { get; set; }           // "deliverable", "undeliverable", "unknown"
		public string reason { get; set; }           // "accepted_email", "rejected_email", etc.
		public bool disposable { get; set; }         // true if it's a temporary email
		public bool role { get; set; }               // true if it's like admin@, info@, etc.
		public string did_you_mean { get; set; }     // Suggestions like "gmail.com" if typo
	}
}
