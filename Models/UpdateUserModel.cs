namespace IdealTrip.Models
{
	public class UpdateUserModel
	{
        public string FullName { get; set; }
        public string Address { get; set; }
        public IFormFile ProfilePhoto { get; set; }
    }
}
