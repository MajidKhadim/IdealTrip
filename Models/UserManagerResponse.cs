namespace IdealTrip.Models
{
	public class UserManagerResponse
	{
		public string Messege {  get; set; }
		public bool IsSuccess { get; set; }
		public IEnumerable<string> Errors { get; set; }
		public DateTime? Expiry {  get; set; }

		public object Data { get; set; }
	}
}
