using IdealTrip.Models.Hotels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using IdealTrip.Models;

namespace IdealTrip.Controllers
{
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	[Authorize(Roles = "HotelOwner")]
	[Route("api/[controller]")]
	[ApiController]
	public class HotelController : ControllerBase
	{
		private readonly ApplicationDbContext _context;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public HotelController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
		{
			_context = context;
			_httpContextAccessor = httpContextAccessor;
		}

		// 1. Add Hotel
		[HttpPost]
		public async Task<IActionResult> AddHotel([FromBody] Hotel hotel)
		{
			var ownerId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(ownerId))
				return Unauthorized(new { IsSuccess = false, Message = "Unauthorized!" });

			hotel.HotelId = Guid.NewGuid();
			hotel.OwnerId = Guid.Parse(ownerId);
			hotel.CreatedAt = DateTime.Now;
			hotel.IsAvailable = true;

			await _context.Hotels.AddAsync(hotel);
			await _context.SaveChangesAsync();

			return Ok(new { IsSuccess = true, Message = "Hotel added successfully", HotelId = hotel.HotelId });
		}

		// 2. Update Hotel
		[HttpPut("{hotelId}")]
		public async Task<IActionResult> UpdateHotel(Guid hotelId, [FromBody] Hotel updatedHotel)
		{
			var ownerId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
			var hotel = await _context.Hotels.FirstOrDefaultAsync(h => h.HotelId == hotelId && h.OwnerId == Guid.Parse(ownerId));
			if (hotel == null)
				return NotFound(new { IsSuccess = false, Message = "Hotel not found!" });

			hotel.HotelName = updatedHotel.HotelName;
			hotel.HotelDescription = updatedHotel.HotelDescription;
			hotel.Address = updatedHotel.Address;

			_context.Hotels.Update(hotel);
			await _context.SaveChangesAsync();

			return Ok(new { IsSuccess = true, Message = "Hotel updated successfully" });
		}

		// 3. Delete Hotel
		[HttpDelete("{hotelId}")]
		public async Task<IActionResult> DeleteHotel(Guid hotelId)
		{
			var ownerId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
			var hotel = await _context.Hotels.FirstOrDefaultAsync(h => h.HotelId == hotelId && h.OwnerId == Guid.Parse(ownerId));
			if (hotel == null)
				return NotFound(new { IsSuccess = false, Message = "Hotel not found!" });

			_context.Hotels.Remove(hotel);
			await _context.SaveChangesAsync();

			return Ok(new { IsSuccess = true, Message = "Hotel deleted successfully" });
		}

		// 4. Get My Hotels
		[HttpGet("my-hotels")]
		public async Task<IActionResult> GetMyHotels()
		{
			var ownerId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
			var hotels = await _context.Hotels
				.Where(h => h.OwnerId == Guid.Parse(ownerId))
				.Select(h => new
				{
					h.HotelId,
					h.HotelName,
					h.HotelDescription,
					h.Address,
					h.IsAvailable,
					h.CreatedAt
				})
				.ToListAsync();

			return Ok(new { IsSuccess = true, Hotels = hotels });
		}
	}
}
