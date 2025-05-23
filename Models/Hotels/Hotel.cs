﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdealTrip.Models.Hotels
{
	public class Hotel
	{
		[Key]
		public Guid HotelId { get; set; }
		[Required]
		public string HotelName { get; set; }
		[Required]
		[StringLength(100)]
		public string HotelDescription { get; set; }
		[Required]
		[StringLength(50)]
		public string Address { get; set; }
		public bool IsAvailable { get; set; } = true;
		public DateTime CreatedAt { get; set; } = DateTime.Now;
		public DateTime UpdatedAt {  get; set; } = DateTime.Now;
		public float Rating { get; set; }
		[ForeignKey("Owner")]
		public Guid OwnerId { get; set; }
		public virtual ApplicationUser Owner { get; set; }
		public bool IsDeleted { get; set; } = false;

    }
}
