using System.ComponentModel.DataAnnotations;

namespace IdealTrip.Models
{
	public class ServiceImage
	{
		[Key]
		public Guid Id { get; set; }

		[Required]
		public Guid ServiceId { get; set; } // FK to LocalHome, Hotel, Transport, etc.

		[Required]
		public string ServiceType { get; set; } // Enum value like "LocalHome", "Hotel", "Transport"

		[Required]
		public string ImageUrl { get; set; } // Azure Storage URL

		public bool IsPrimary { get; set; } = false;

		public DateTime UploadedAt { get; set; } = DateTime.Now;
	}
}
