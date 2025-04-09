using System.ComponentModel.DataAnnotations;

namespace IdealTrip.Models.Package_Booking
{
	public class Package
	{
        public Guid PackageId { get; set; }
        [Required]
        public string Thumbnail { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public int DurationDays { get; set; }
        public int Price { get; set; }
        [Required]
        public string Description { get; set; }
        public string Tag { get; set; }
        [Required]
        
        public DateOnly TourDate { get; set; }
        [Required]
        public int AvailableSpots { get; set; }
    }
}
