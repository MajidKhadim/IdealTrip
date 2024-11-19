using System.ComponentModel.DataAnnotations;

namespace IdealTrip.Models.Register
{
    public class RegisterWithProofDto : RegisterDtoBase
    {
        [Required]
        public string DocumentType { get; set; }
        [Required]
        public ICollection<IFormFile> ProofDocuments { get; set; }
    }
}
