using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace IdealTrip.Models.TourGuide_Booking
{
	public class TourGuide
	{
		[Key]
		public Guid Id { get; set; }

		[Required]
		public Guid UserId { get; set; }

		[ForeignKey("UserId")]
		public ApplicationUser User { get; set; } // ✅ Navigation property

		[Required, MaxLength(100)]
		public string FullName { get; set; }

		[Required, MaxLength(20)]
		public string PhoneNumber { get; set; }

		[Required, Column(TypeName = "decimal(10,2)")]
		public decimal HourlyRate { get; set; }

		[MaxLength(500)]
		public string Experience { get; set; }

		[MaxLength(1000)]
		public string Bio { get; set; }

		[Required, MaxLength(100)]
		public string Location { get; set; }

		public bool IsAvailable { get; set; }

		[Range(0, 5)]
		public float Rating { get; set; } // ✅ Average rating (0-5)
	}
}
