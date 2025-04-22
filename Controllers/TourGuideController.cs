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
using IdealTrip.Models.Database_Tables;
using Microsoft.AspNetCore.SignalR;
using System;

namespace IdealTrip.Controllers
{
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	[Authorize(Roles = "Tourist")]
	[Route("api/[controller]")]
	[ApiController]

	public class TourGuideController : ControllerBase
	{
		private readonly ApplicationDbContext _context;
		private readonly ILogger<TourGuideController> _logger;
		private readonly IHttpContextAccessor _contextAccessor;
		private readonly PaymentService _paymentService;
		private readonly EmailService _emailService;
		private readonly IHubContext<NotificationHub> _hubContext;
		public TourGuideController(ApplicationDbContext dbContext,ILogger<TourGuideController> logger, IHttpContextAccessor contextAccessor, PaymentService paymentService, EmailService emailService, IHubContext<NotificationHub> hubContext)
		{
			_context = dbContext;
			_logger = logger;
			_contextAccessor = contextAccessor;
			_paymentService = paymentService;
			_emailService = emailService;
			_hubContext = hubContext;
		}
		[AllowAnonymous]
		[HttpGet]
		public async Task<IActionResult> GetAllTourGuides(
	[FromQuery] string? location,
	[FromQuery] double? minimumRating,
	[FromQuery] decimal? maximumRatePerDay,
	[FromQuery] DateTime? startDate,
	[FromQuery] DateTime? endDate)
		{
			try
			{
				var query = _context.TourGuides
					.Include(tg => tg.User)
					.Where(tg => tg.IsAvailable && tg.User.Status == Models.Enums.ProofStatus.Verified && !tg.User.IsDeleted)
					.AsQueryable();

				// ✅ Apply filters
				if (!string.IsNullOrEmpty(location))
				{
					query = query.Where(tg => tg.Location.ToLower().Contains(location.ToLower()));
				}

				if (minimumRating.HasValue)
				{
					query = query.Where(tg => tg.Rating >= minimumRating.Value);
				}

				if (maximumRatePerDay.HasValue)
				{
					query = query.Where(tg => tg.RatePerDay <= maximumRatePerDay.Value);
				}

				// ✅ Exclude guides who are already booked during the selected dates
				if (startDate.HasValue && endDate.HasValue)
				{
					var bookedGuideIds = await _context.UserTourGuideBookings
						.Where(b => b.Status == BookingStatus.Paid.ToString()
							&& startDate <= b.EndDate && endDate >= b.StartDate)
						.Select(b => b.TourGuideId)
						.ToListAsync();

					query = query.Where(tg => !bookedGuideIds.Contains(tg.Id));
				}

				// ✅ Project final result
				var tourGuides = await query
					.Select(tg => new
					{
						tg.Id,
						tg.FullName,
						tg.PhoneNumber,
						tg.RatePerDay,
						tg.Experience,
						tg.Bio,
						tg.Location,
						tg.Rating,
						tg.User.ProfilePhotoPath,
					})
					.ToListAsync();

				return Ok(new UserManagerResponse
				{
					IsSuccess = true,
					Data = tourGuides,
					Messege = "Tour guides retrieved successfully!"
				});
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error while fetching tour guides data: {ex.Message}");
				return StatusCode(500, new DataSendingResponse
				{
					IsSuccess = false,
					Message = "Internal Server Error",
					Errors = new List<string> { ex.Message }
				});
			}
		}
		[AllowAnonymous]
		[HttpGet("{id}")]
		public async Task<IActionResult> GetTourGuideDetails(Guid id)
		{

			try
			{
				var tourGuide = await _context.TourGuides.Where(tg => tg.Id == id && !tg.User.IsDeleted)
				.Select(tg => new
				{
					tg.Id,
					tg.FullName,
					tg.PhoneNumber,
					tg.RatePerDay,
					tg.Experience,
					tg.Bio,
					tg.Location,
					tg.Rating,
					tg.User.ProfilePhotoPath,
				}).FirstOrDefaultAsync();
				if (tourGuide == null)
				{
					return NotFound(new UserManagerResponse
					{
						IsSuccess = true,
						Messege = "Tour Guide Not Found"
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
				return StatusCode(500, new DataSendingResponse
				{
					IsSuccess = false,
					Message = "Internal Server Error",
					Errors = new List<string> { ex.Message }
				});
			}
		}
		[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		[Authorize(Roles = "Tourist")]
		[HttpPost("booking/initiate")]
		public async Task<IActionResult> InitiateBooking([FromBody] TourGuideBookingModel booking)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(new UserManagerResponse
				{
					IsSuccess = false,
					Messege = "Invalid Data",
					Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
				});
			}
			try
			{
				var userId = _contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
				if (string.IsNullOrEmpty(userId))
				{
					return Unauthorized(new { IsSuccess = false, Message = "Invalid Token!" });
				}

				if (booking == null || booking.TotalDays <= 0 || booking.StartDate == default || booking.EndDate == default)
				{
					return BadRequest(new { IsSuccess = false, Message = "Invalid booking details!" });
				}

				var tourGuide = await _context.TourGuides.FindAsync(booking.TourGuideId);
				if (tourGuide == null || !tourGuide.IsAvailable)
				{
					return BadRequest(new { IsSuccess = false, Message = "Tour guide not available!" });
				}

				// 🛑 Check for conflicting bookings (with "Paid" status)
				// 🛑 Get all conflicting bookings (with "Paid" status)
				var conflictingBookings = await _context.UserTourGuideBookings
					.Where(b => b.TourGuideId == booking.TourGuideId && b.Status == BookingStatus.Paid.ToString())
					.Where(b => booking.StartDate <= b.EndDate && booking.EndDate >= b.StartDate)
					.Select(b => new
					{
						b.StartDate,
						b.EndDate
					})
					.ToListAsync();

				if (conflictingBookings.Any())
				{
					return BadRequest(new
					{
						IsSuccess = false,
						Message = "Tour guide is already booked for the selected dates!",
						ConflictingDates = conflictingBookings
					});
				}


				// ✅ Calculate total cost
				decimal totalCost = booking.TotalDays * tourGuide.RatePerDay;

				var pendingBooking = new UserTourGuideBooking
				{
					UserId = Guid.Parse(userId),
					TourGuideId = booking.TourGuideId,
					TotalAmount = totalCost,
					Status = BookingStatus.Pending.ToString(),
					TotalDays = booking.TotalDays,
					StartDate = booking.StartDate,
					EndDate = booking.EndDate,
					BookingDate = DateTime.Now
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
			catch (Exception ex) 
			{
				return StatusCode(500, new DataSendingResponse
				{
					IsSuccess = false,
					Message = "Internal Server Error",
					Errors = new List<string> { ex.Message }
				});
			}
			}


		
		[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		[Authorize(Roles = "Tourist")]
		[HttpPost("booking/payment-success")]
		public async Task<IActionResult> PaymentSuccess([FromBody] PaymentSuccessDto paymentData)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(new UserManagerResponse
				{
					IsSuccess = false,
					Messege = "Invalid Data",
					Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
				});
			}
			try
			{
				if (paymentData == null || string.IsNullOrEmpty(paymentData.BookingId) || string.IsNullOrEmpty(paymentData.PaymentIntentId))
				{
					return BadRequest(new { IsSuccess = false, Message = "Invalid request data." });
				}

				var bookingId = Guid.Parse(paymentData.BookingId);
				var booking = await _context.UserTourGuideBookings
											.Include(b => b.User)
											.Include(b => b.TourGuide)
											.FirstOrDefaultAsync(b => b.Id == bookingId);

				if (booking == null)
				{
					return NotFound(new { IsSuccess = false, Message = "Booking not found." });
				}

				using var transaction = await _context.Database.BeginTransactionAsync();
				try
				{
					// ✅ Update booking status and store PaymentIntentId
					booking.Status = BookingStatus.Paid.ToString();
					booking.PaymentIntentId = paymentData.PaymentIntentId;

					_context.UserTourGuideBookings.Update(booking);
					await _context.SaveChangesAsync();

					// ✅ Real-time Notification to Tour Guide (with Booking Date)
					//var tourGuideNotificationMessage = $"📅 You have a new booking for {booking.BookingDate:MMMM dd, yyyy} from {booking.User.FullName}! 💳 Payment ID: {paymentData.PaymentIntentId}";
					//var tourGuideNotification = new Notifications
					//{
					//	UserId = booking.TourGuide.User.Id,
					//	Messege = tourGuideNotificationMessage
					//};
					//_context.Notifications.Add(tourGuideNotification);
					//await _hubContext.Clients.User(booking.TourGuide.Id.ToString())
					//				  .SendAsync("ReceiveNotification", tourGuideNotificationMessage);

					// ✅ Real-time Notification to User
					var userNotificationMessage = $"🎉 Your booking with {booking.TourGuide.FullName} on {booking.BookingDate:MMMM dd, yyyy} is confirmed! 💳 Payment ID: {paymentData.PaymentIntentId}";
					var userNotification = new Notifications
					{
						UserId = booking.User.Id,
						Messege = userNotificationMessage
					};
					_context.Notifications.Add(userNotification);
					await _hubContext.Clients.User(booking.User.Id.ToString())
									  .SendAsync("ReceiveNotification", userNotificationMessage);

					// ✅ Notify All Users (if needed)
					await _hubContext.Clients.All.SendAsync("ReceiveNotification",
						$"📢 {booking.User.FullName} just booked a tour with {booking.TourGuide.FullName} for {booking.BookingDate:MMMM dd, yyyy}!");

					await _context.SaveChangesAsync();
					var tourguide = await _context.TourGuides
	.Include(tg => tg.User)
	.FirstOrDefaultAsync(tg => tg.Id == booking.TourGuideId);




					// ✅ Send Email Confirmation
					string emailContent = EmailTemplates.TourGuideBookingSuccessTemplate(
	booking.User.FullName,
	booking.TourGuide.FullName,
	booking.TourGuide.Bio,
	booking.StartDate.ToString("dd MMM yyyy"),
	booking.EndDate.ToString("dd MMM yyyy"),
	booking.NumberOfTravelers,
	booking.TotalAmount,
	booking.PaymentIntentId,
	booking.TotalDays,
	booking.Status,
	booking.BookingDate
);


					await _emailService.SendEmailAsync(booking.User.Email, "Booking Successful", emailContent);
					string guideEmailContent = EmailTemplates.TourGuideBookingNotificationTemplate(
	booking.TourGuide.FullName,
	booking.User.FullName,
	booking.User.Email,
	booking.User.PhoneNumber,
	booking.StartDate,
	booking.EndDate,
	booking.NumberOfTravelers,
	booking.TotalAmount,
	booking.PaymentIntentId,
	booking.TotalDays,
	booking.BookingDate,
	booking.SpecialRequest
);

					await _emailService.SendEmailAsync(tourguide.User.Email, "You've Got a New Booking!", guideEmailContent);


					await transaction.CommitAsync();
					return Ok(new { IsSuccess = true, Message = "Payment updated, notifications sent successfully." });
				}
				catch (Exception ex)
				{
					await transaction.RollbackAsync();
					return BadRequest(new DataSendingResponse { IsSuccess = false, Message = "Payment processing failed.", Errors = new List<string> { ex.Message } });
				}
			}
			catch (Exception ex) 
			{
				return StatusCode(500, new DataSendingResponse
				{
					IsSuccess = false,
					Message = "Internal Server Error",
					Errors = new List<string> { ex.Message }
				});
			}
		}

		[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		[Authorize]
		[HttpGet("user-bookings")]
		public async Task<IActionResult> GetUserBookings([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
		{
			try
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
			catch (Exception ex) 
			{
				return StatusCode(500, new DataSendingResponse
				{
					IsSuccess = false,
					Message = "Internal Server Error",
					Errors = new List<string> { ex.Message }
				});
			}
		}
		// POST: Add Feedback for
		[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		[Authorize(Roles = "Tourist")]
		[HttpPost("add-feedback")]
		public async Task<IActionResult> AddFeedback([FromBody] FeedbackRequest request)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(new UserManagerResponse
				{
					IsSuccess = false,
					Messege = "Invalid Data",
					Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
				});
			}
			try
			{
				var userId = _contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
				if (string.IsNullOrEmpty(userId))
				{
					return Unauthorized(new DataSendingResponse { IsSuccess = false, Message = "Unauthorized action." });
				}

				// Check if tour guide exists
				var tourGuide = await _context.TourGuides.FirstOrDefaultAsync(tg => tg.Id == request.ServiceId && !tg.User.IsDeleted);
				if (tourGuide == null)
				{
					return NotFound(new DataSendingResponse
					{
						IsSuccess = false,
						Message = "Tour guide not found."
					});
				}

				// ✅ Ensure user has a PAID booking with the tour guide AND the tour is completed
				var hasCompletedTour = await _context.UserTourGuideBookings.AnyAsync(b =>
					b.UserId == Guid.Parse(userId) &&
					b.TourGuideId == request.ServiceId &&
					b.Status == BookingStatus.Paid.ToString() &&
					b.EndDate <= DateTime.UtcNow);

				if (!hasCompletedTour)
				{
					return BadRequest(new DataSendingResponse
					{
						IsSuccess = false,
						Message = "You can only give feedback after completing the tour."
					});
				}

				// ✅ Add feedback (multiple feedbacks allowed)
				var feedback = new Feedback
				{
					Id = Guid.NewGuid(),
					UserId = Guid.Parse(userId),
					ServiceType = Service.TourGuide.ToString(),
					ServiceId = request.ServiceId,
					FeedbackText = request.FeedbackText,
					Rating = request.Rating,
					Date = DateTime.UtcNow
				};

				_context.FeedBacks.Add(feedback);
				await _context.SaveChangesAsync();

				// ✅ Update average rating
				var averageRating = await _context.FeedBacks
					.Where(f => f.ServiceId == request.ServiceId && f.ServiceType == Service.TourGuide.ToString())
					.AverageAsync(f => (decimal?)f.Rating) ?? 0;

				tourGuide.Rating = (float)averageRating;
				_context.TourGuides.Update(tourGuide);
				await _context.SaveChangesAsync();

				return Ok(new DataSendingResponse
				{
					IsSuccess = true,
					Data = feedback,
					Message = "Feedback submitted and rating updated successfully!"
				});
			}
			catch (Exception ex) 
			{
				return StatusCode(500, new DataSendingResponse
				{
					IsSuccess = false,
					Message = "Internal Server Error",
					Errors = new List<string> { ex.Message }
				});
			}
		}

		// GET: Get Feedback for Tour Guide
		[HttpGet("get-feedback/{tourGuideId}")]
		public async Task<IActionResult> GetFeedback(Guid tourGuideId)
		{
			try {
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
			catch (Exception ex)
			{
				return StatusCode(500, new DataSendingResponse
				{
					IsSuccess = false,
					Message = "Internal Server Error",
					Errors = new List<string> { ex.Message }
				});
			}
		}
	}

}
