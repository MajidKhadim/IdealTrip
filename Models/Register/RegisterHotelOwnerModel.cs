using System.ComponentModel.DataAnnotations;

namespace IdealTrip.Models.Register
{
    public class RegisterHotelOwnerModel : RegisterWithProofDto
    {
		[Required]
		public IFormFile PropertyOwnerShipDoc { get; set; }
		public ICollection<IFormFile> ImageGalleryDoc { get; set; }
	}
}
