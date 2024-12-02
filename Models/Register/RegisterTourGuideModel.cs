using System.ComponentModel.DataAnnotations;

namespace IdealTrip.Models.Register
{
    public class RegisterTourGuideModel : RegisterWithProofDto
    {
        [Required]
        public IFormFile IdCard { get; set; }
    }
}
