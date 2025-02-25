using IdealTrip.Helpers;
using IdealTrip.Models;
using IdealTrip.Models.Enums;
using IdealTrip.Models.Package_Booking;
using IdealTrip.Models.TourGuide_Booking;
using IdealTrip.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using IdealTrip.Models.Enums;

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
		private readonly PaymentService _paymentService;
		private readonly EmailService _emailService;
		public TourGuideController(ApplicationDbContext dbContext,ILogger<TourGuideController> logger, IHttpContextAccessor contextAccessor, PaymentService paymentService, EmailService emailService)
		{
			_context = dbContext;
			_logger = logger;
			_contextAccessor = contextAccessor;
			_paymentService = paymentService;
			_emailService = emailService;
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
					tg.RatePerDay,
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
					tg.RatePerDay,
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
		public async Task<IActionResult> InitiateBooking([FromBody] TourGuideBookingModel booking)
		{
			var userId = _contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized(new { IsSuccess = false, Message = "Invalid Token!" });
			}

			if (booking == null || booking.TotalDays <= 0)
			{
				return BadRequest(new { IsSuccess = false, Message = "Invalid booking details!" });
			}

			var tourGuide = await _context.TourGuide.FindAsync(booking.TourGuideId);
			if (tourGuide == null || !tourGuide.IsAvailable)
			{
				return BadRequest(new { IsSuccess = false, Message = "Tour guide not available!" });
			}

			// Calculate total cost
			decimal totalCost = booking.TotalDays * tourGuide.RatePerDay;

			var pendingBooking = new UserTourGuideBooking
			{
				UserId = Guid.Parse(userId),
				TourGuideId = booking.TourGuideId,
				TotalAmount = totalCost,
				Status = "Pending",
				TotalDays = booking.TotalDays
			};

			_context.UserTourGuideBookings.Add(pendingBooking);
			await _context.SaveChangesAsync();

			var paymentResult = await _paymentService.CreatePaymentIntent(pendingBooking.Id, "TourGuide", pendingBooking.TotalAmount);
			var clientSecret = paymentResult?.GetType().GetProperty("clientSecret")?.GetValue(paymentResult)?.ToString();

			if (!string.IsNullOrEmpty(clientSecret))
			{
				return Ok(new { IsSuccess = true, BookingId = pendingBooking.Id, ClientSecret = clientSecret });
			}
			else
			{
				return BadRequest(new { IsSuccess = false, Message = "Failed to create payment intent!" });
			}
		}

		[HttpPost("booking/payment-success")]
		public async Task<IActionResult> PaymentSuccess([FromBody] PaymentSuccessDto paymentData)
		{
			if (paymentData == null || string.IsNullOrEmpty(paymentData.BookingId) || string.IsNullOrEmpty(paymentData.PaymentIntentId))
			{
				return BadRequest(new { IsSuccess = false, Message = "Invalid request data." });
			}

			var bookingId = Guid.Parse(paymentData.BookingId);
			var booking = await _context.UserTourGuideBookings.FindAsync(bookingId);

			if (booking == null)
			{
				return NotFound(new { IsSuccess = false, Message = "Booking not found." });
			}

			booking.Status = "Paid";
			booking.PaymentIntentId = paymentData.PaymentIntentId;
			_context.UserTourGuideBookings.Update(booking);
			await _context.SaveChangesAsync();
			string content = EmailTemplates.PaymentSuccessTemplate(booking.User.FullName, booking.TotalAmount.ToString(), booking.BookingDate.ToString(), booking.TourGuide.FullName, booking.TourGuide.Bio, booking.Status, booking.PaymentIntentId);
			await _emailService.SendEmailAsync(booking.User.Email, "Booking Successful", content);

			return Ok(new { IsSuccess = true, Message = "Payment updated successfully. Confirmation email sent." });
		}

		[HttpGet("user-bookings")]
		public async Task<IActionResult> GetUserBookings([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
		{
			var userId = _contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized(new { IsSuccess = false, Message = "Invalid Token!" });
			}

			var query = _context.UserTourGuideBookings
				.Where(b => b.UserId == Guid.Parse(userId))
				.OrderByDescending(b => b.Status)
				.Skip((page - 1) * pageSize)
				.Take(pageSize);

			var bookings = await query.Select(b => new
			{
				b.Id,
				b.TourGuideId,
				b.TotalDays,
				b.TotalAmount,
				b.Status,
				b.PaymentIntentId
			}).ToListAsync();

			return Ok(new { IsSuccess = true, Bookings = bookings });
		}
		// POST: Add Feedback for Tour Guide
		[HttpPost("add-feedback")]
		public async Task<IActionResult> AddFeedback([FromBody] FeedbackRequest request)
		{
			var userId = _contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized(new DataSendingResponse { IsSuccess = false, Message = "Unauthorized action." });
			}

			var tourGuide = await _context.TourGuide.FirstOrDefaultAsync(tg => tg.Id == request.ServiceId);
			if (tourGuide == null)
			{
				return NotFound(new DataSendingResponse
				{
					IsSuccess = false,
					Message = "Tour guide not found."
				});
			}

			var feedback = new Feedback
			{
				Id = Guid.NewGuid(),
				UserId = Guid.Parse(userId),
				ServiceType = Service.TourGuide.ToString(),
				ServiceId = request.ServiceId,
				FeedbackText = request.FeedbackText,
				Rating = request.Rating,
				Date = DateTime.Now
			};

			_context.FeedBacks.Add(feedback);
			await _context.SaveChangesAsync();

			// Update average rating
			var averageRating = await _context.FeedBacks
				.Where(f => f.ServiceId == request.ServiceId && f.ServiceType == Service.TourGuide.ToString())
				.AverageAsync(f => (decimal?)f.Rating) ?? 0;

			tourGuide.Rating = ((float)averageRating);
			_context.TourGuide.Update(tourGuide);
			await _context.SaveChangesAsync();

			return Ok(new DataSendingResponse
			{
				IsSuccess = true,
				Data = feedback,
				Message = "Feedback added and rating updated successfully."
			});
		}

		// GET: Get Feedback for Tour Guide
		[HttpGet("get-feedback/{tourGuideId}")]
		public async Task<IActionResult> GetFeedback(Guid tourGuideId)
		{
			var feedbacks = await _context.FeedBacks
			.Where(f => f.ServiceId == tourGuideId && f.ServiceType == Service.TourGuide.ToString())
			.OrderByDescending(f => f.Date)
			.Select(f => new {
				f.FeedbackText,
				f.Rating,
				f.Date,
				User = new
				{
					f.User.Id,
					f.User.FullName
				}
			})
			.ToListAsync();


			return Ok(new DataSendingResponse
			{
				IsSuccess = true,
				Data = feedbacks,
				Message = "Feedback retrieved successfully."
			});
		}
	}

}
