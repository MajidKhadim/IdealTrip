using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace IdealTrip.Models.LocalHome_Booking
{
	public class AddLocalHomeModel
	{
		[Required]
		[MaxLength(100)]
		public string Name { get; set; }

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
		public int Capacity { get; set; }

		[Required]
		public DateTime AvailableFrom { get; set; }

		[Required]
		public DateTime AvailableTo { get; set; }

        //public string CancellationPolicy { get; set; }

        //[Required]
        //public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Pending;
        [Required]
        public ICollection<IFormFile> Images { get; set; }
    }
}
