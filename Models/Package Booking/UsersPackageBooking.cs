using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdealTrip.Models.Package_Booking
{
	public class UsersPackageBooking
	{
		public Guid Id { get; set; }
		[Required]
		public Guid UserId { get; set; }
		[Required]
		public string FullName { get; set; }
		[Required]
		public string Email { get; set; }
		[Required]
		public string PhoneNumber { get; set; }
		[Required]
		public int NumberOfTravelers { get; set; }
        public Guid PackageId { get; set; }
        [ForeignKey("PackageId")]
		public Package Package { get; set; }
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
        public string Status { get; set; }
        public decimal TotalBill { get; set; }
		public DateTime BookingDate { get; set; } = DateTime.Now;
	}
}
