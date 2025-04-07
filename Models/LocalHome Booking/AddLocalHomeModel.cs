using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

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

		[Required]
		[Column(TypeName = "decimal(18,2)")]
		public decimal PricePerNight { get; set; }
        [Required]
        public int NumberOfRooms { get; set; }

        public int Capacity { get; set; }

		[Required]
		public DateOnly AvailableFrom { get; set; }

		[Required]
		public DateOnly AvailableTo { get; set; }

		/// <summary>
		/// Primary image for the local home
		/// </summary>
		[Required]
		public IFormFile PrimaryImage { get; set; }

		/// <summary>
		/// Additional images for the local home
		/// </summary>
		public ICollection<IFormFile> Images { get; set; }
	}
}
