namespace IdealTrip.Models
{
	public class UpdateUserDto
	{
        public string FullName { get; set; }
        public string Address { get; set; }
        public IFormFile ProfilePhoto { get; set; }
    }
}
