using System.ComponentModel.DataAnnotations;

namespace IdealTrip.Models.Register
{
    public class RegisterLocalHomeOwnerModel : RegisterWithProofDto
    {
        [Required]
        public IFormFile PropertyOwnerShipDoc { get; set; }
        [Required]
        public ICollection<IFormFile> ImageGalleryDoc { get; set; }
    }
}
