namespace IdealTrip.Models
{
	public class DataSendingResponse
	{
		public string Message { get; set; }
		public bool IsSuccess { get; set; }
		public IEnumerable<string> Errors { get; set; }
		public Object Data { get; set; }
	}
}
