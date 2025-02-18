using IdealTrip.Models;
using IdealTrip.Models.Package_Booking;
using IdealTrip.Models.TourGuide_Booking;
using IdealTrip.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace IdealTrip.Controllers
{
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	[Authorize("Tourist")]
	[Route("api/[controller]")]
	[ApiController]

	public class TourGuideController : ControllerBase
	{
		private readonly ApplicationDbContext _context;
		private readonly ILogger<TourGuideController> _logger;
		private readonly IHttpContextAccessor _contextAccessor;
		public TourGuideController(ApplicationDbContext dbContext,ILogger<TourGuideController> logger, IHttpContextAccessor contextAccessor)
		{
			_context = dbContext;
			_logger = logger;
			_contextAccessor = contextAccessor;
		}
		[HttpGet]
		public async Task<IActionResult> GetAllTourGuides()
		{
			try
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
				return Ok(new UserManagerResponse
				{
					IsSuccess = true,
					Data = tourGuides,
					Messege = "TourGuides retrived Successfully!"
				});

			}
			catch (Exception ex)
			{
				_logger.LogError($"Error while fetching tour guides data : {ex.Message}");
				return StatusCode(500, new UserManagerResponse
				{
					IsSuccess = false,
					Messege = "Something went wrong!"
				});
			}
		}
		[HttpGet("{id}")]
		public async Task<IActionResult> GetTourGuideDetails(Guid id)
		{

			try
			{
				var tourGuide = await _context.TourGuide.Where(tg => tg.Id == id)
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
				}).FirstOrDefaultAsync();
				if (tourGuide == null)
				{
					return NotFound(new UserManagerResponse
					{
						IsSuccess = false,
						Messege = "Tour Guide NOt Found"
					});
				}
				return Ok(new UserManagerResponse
				{
					IsSuccess = true,
					Messege = "Tour Guide retrieved Successfully",
					Data = tourGuide
				});
			}
			catch(Exception ex)
			{
				_logger.LogError($"Error while fetching Tour Guide data : {ex.Message}");
				return StatusCode(500, new UserManagerResponse
				{
					IsSuccess = false,
					Messege = "Internal Server Error"
				});
			}
		}
		[HttpPost("booking/initiate")]
		public async Task<IActionResult> InitiateBooking([FromBody] PackageBookingModel booking)
		{
			var userId = _contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized("Invalid Token!");
			}

			var package = await _context.Packages.FindAsync(booking.PackageId);
			if (package == null || package.AvailableSpots < booking.NumberOfTravelers)
				return BadRequest(new { IsSuccess = false, Message = "Invalid package or insufficient spots!" });

			// Save booking with "Pending" status
			var pendingBooking = new UsersPackageBooking
			{
				UserId = Guid.Parse(userId),
				FullName = booking.FullName,
				Email = booking.Email,
				PhoneNumber = booking.PhoneNumber,
				NumberOfTravelers = booking.NumberOfTravelers,
				PackageId = booking.PackageId,
				TotalBill = booking.TotalBill,
				Status = "Pending",
			};

			_context.UsersPackages.Add(pendingBooking);
			await _context.SaveChangesAsync();


			// Call Payment Service
			var sessionUrl = await PaymentService.CreateCheckoutSession(
				pendingBooking.Id,
				"Package",
				pendingBooking.TotalBill,
				"pkr",
				"",
				""
			);

			return Ok(new { IsSuccess = true, BookingId = pendingBooking.Id, PaymentUrl = sessionUrl });
		}
	}
}
