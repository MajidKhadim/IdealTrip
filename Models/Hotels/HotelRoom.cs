using IdealTrip.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdealTrip.Models.Hotels
{
    public class HotelRoom
    {
        [Key]
        public Guid RoomId { get; set; }
        public RoomType RoomType { get; set; }
        [Required]
        public decimal PricePerNight { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Capacity Can't be 0 or less than 0")]
        [Required]
        public int Capacity { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "NumberOfBeds Can't be 0 or less than 0")]
        [Required]
        public int NumberOfBeds { get; set; }
        public bool IsAvailable { get; set; } = true;

        [ForeignKey("Hotel")]
        public Guid HotelId { get; set; }
        public virtual Hotel Hotel { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}
