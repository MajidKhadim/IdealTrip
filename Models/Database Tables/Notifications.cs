using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdealTrip.Models.Database_Tables
{
	public class Notifications
	{
		[Key]
		public Guid Id { get; set; }
		[Required]
		public Guid UserId { get; set; }
		[Required]
		public string Messege { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
		public bool IsRead { get; set; } = false;
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

    }
}
