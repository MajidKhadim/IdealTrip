using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace IdealTrip.Models.LocalHome_Booking
{
	public class LocalHome
	{
		[Key]
		public Guid Id { get; set; }

		[Required]
		public Guid OwnerId { get; set; }

		[ForeignKey("OwnerId")]
		public ApplicationUser Owner { get; set; } // Assuming ApplicationUser is your user model

		[Required]
		[MaxLength(100)]
		public string Name { get; set; }
		[Required]
		[Range(1, int.MaxValue, ErrorMessage = "NumberOfRooms must be greater than 0.")]
		public int NumberOfRooms { get; set; }
        [Required]
		[MaxLength(1000)]
		public string Description { get; set; }

		[Required]
		public string AddressLine { get; set; }

		//[Required]
		//public string City { get; set; }

		//[Required]
		//public string State { get; set; }

		//[Required]
		//public string PostalCode { get; set; }

		//[Required]
		//public string Location { get; set; }

		//public decimal Latitude { get; set; }
		//public decimal Longitude { get; set; }

		[Required]
		[Column(TypeName = "decimal(18,2)")]
		public decimal PricePerNight { get; set; }

		public float Rating { get; set; }
		public int Capacity { get; set; }

		[Required]
		public DateOnly AvailableFrom { get; set; }

		[Required]
		public DateOnly AvailableTo { get; set; }

		public bool IsAvailable { get; set; }

		//public string CancellationPolicy { get; set; }

		//[Required]
		//public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Pending;

		public DateTime CreatedAt { get; set; } = DateTime.Now;
		public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public bool IsDeleted { get; set; } = false;
    }
}
