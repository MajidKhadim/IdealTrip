using System.ComponentModel.DataAnnotations;

namespace IdealTrip.Models.Register
{
    public class RegisterTourGuideModel : RegisterWithProofDto
    {
        [Required]
        public IFormFile IdCard { get; set; }
        [Required]
        public decimal HourlyRate { get; set; }
        [Required]
        public string Bio { get; set; }
        [Required]
        public string Experience { get; set; }
        [Required]
        public string Location { get; set; }
    }
}
