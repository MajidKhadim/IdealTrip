using IdealTrip.Models.Enums;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace IdealTrip.Models
{
	public class ApplicationUser : IdentityUser<Guid>
	{
		[MaxLength(255)]
		public string ProfilePhotoPath { get; set; } = string.Empty;

		public ProofStatus Status { get; set; } = ProofStatus.Pending;

		[Required]
		[MaxLength(100)]
		public string FullName { get; set; }

		public bool IsEmailConfirmed { get; set; } = false;
		public string Address { get; set; }
		[Required]
		[MaxLength(50)]
		public string Role { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.Now;
		public bool IsEmailBounced { get; set; }
		public string? BounceReason { get; set; }

        public bool IsDeleted { get; set; } = false;
    }

}
