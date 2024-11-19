using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdealTrip.Models
{
	public class Proof
	{
		[Key]
        public Guid DocId { get; set; }
        public Guid UserId { get; set; }
        public string DocumentType { get; set; }
        public string DocumentPath { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
    }
}
