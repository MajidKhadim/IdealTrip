using IdealTrip.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace IdealTrip.Models.Hotels
{
	public class AddHotelRoomModel
	{
		public RoomType RoomType { get; set; }
		[Required]
		public decimal PricePerNight { get; set; }
		[Range(1, int.MaxValue, ErrorMessage = "Capacity Can't be 0 or less than 0")]
		[Required]
		public int Capacity { get; set; }
		[Range(1, int.MaxValue, ErrorMessage = "NumberOfBeds Can't be 0 or less than 0")]
		[Required]
		public int NumberOfBeds { get; set; }
		[Required]
		public IFormFile PrimaryImage { get; set; }
		public ICollection<IFormFile> Images { get; set; }
	}
}
