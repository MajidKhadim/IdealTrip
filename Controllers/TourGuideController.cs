using IdealTrip.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdealTrip.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class TourGuideController : ControllerBase
	{
		private readonly ApplicationDbContext _context;
		TourGuideController(ApplicationDbContext dbContext) 
		{
			_context = dbContext;
		}
		[HttpGet]
		public async Task<IActionResult> GetAllTourGuides()
		{
			var tourGuides = await _context.TourGuide
				.Where(tg => tg.IsAvailable && tg.User.Status == Models.Enums.ProofStatus.Verified)
				.Select(tg => new
				{
					tg.Id,
					tg.FullName,
					tg.PhoneNumber,
					tg.HourlyRate,
					tg.Experience,
					tg.Bio,
					tg.Location,
					tg.Rating
				})
				.ToListAsync();

			return Ok(tourGuides);
		}


	}
}
