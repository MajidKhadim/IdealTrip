using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdealTrip.Models.Hotels
{
    public class UserHotelRoomBooking
    {
        [Key]
        public Guid BookingId { get; set; }
        [ForeignKey("HotelRoom")]
        public Guid RoomId { get; set; }
        [ForeignKey("User")]
        public Guid UserId { get; set; }
        public virtual ApplicationUser Tourist { get; set; }
        public virtual HotelRoom HotelRoom { get; set; }
        [Required]
        public DateOnly CheckInDate { get; set; }
        [Required]
        public DateOnly CheckOutDate { get; set; }

        public int TotalDays { get; set; }
        [Required]
        public string PaymentIntentId { get; set; }
        public string Status { get; set; } // "Pending", "Paid", "Cancelled"
        public DateTime BookingTime { get; set; }

    }
}
