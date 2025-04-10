using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace IdealTrip.Models.Hotels
{
	public class AddHotelModel
	{
		[Required]
		public string HotelName { get; set; }
		[Required]
		[StringLength(100)]
		public string HotelDescription { get; set; }
		[Required]
		[StringLength(50)]
		public string Address { get; set; }
        public bool IsAvailable { get; set; }
        [Required]
		public IFormFile PrimaryImage { get; set; }
		public ICollection<IFormFile> Images { get; set; }
	}
}
