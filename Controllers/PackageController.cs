using IdealTrip.Helpers;
using IdealTrip.Models;
using IdealTrip.Models.Database_Tables;
using IdealTrip.Models.Package_Booking;
using IdealTrip.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Stripe.Checkout;
using System.Security.Claims;

namespace IdealTrip.Controllers
{
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	[Authorize(Roles = "Tourist")]
	[Route("api/[controller]")]
	
    public class PackageController : ControllerBase
    {
		private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PackageController> _logger;
		private readonly IConfiguration _config;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly PaymentService _paymentService;
		private readonly EmailService _emailService;
        public PackageController(ApplicationDbContext context, ILogger<PackageController> logger,IConfiguration configuration,IHttpContextAccessor httpContextAccessor,PaymentService service,IHubContext<NotificationHub> hubContext,EmailService emailService)
        {
            _context = context;
            _logger = logger;
			_config = configuration;
			_httpContextAccessor = httpContextAccessor;
			_paymentService = service;
			_hubContext = hubContext;
			_emailService = emailService;
        }

        [HttpGet]
		[AllowAnonymous]
        public async Task<IActionResult> GetPackages()
        {
            try
            {
                var packages = await _context.Packages.ToListAsync();
                return Ok(new UserManagerResponse
                {
                    IsSuccess = true,
                    Data = packages,
                    Messege = "Packages retrived Successfully!"
                });

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while fetching packages data : {ex.Message}");
                return StatusCode(500, new UserManagerResponse
                {
                    IsSuccess = false,
                    Messege = "Something went wrong!"
                });
            }
        }
        [HttpGet("{id}")]
		public async Task<IActionResult> GetPackage(string id)
		{
			try
			{
				var package = await _context.Packages.FirstOrDefaultAsync(p => p.PackageId.ToString() == id);
                if (package != null)
                {
                    return Ok(new UserManagerResponse
                    {
                        IsSuccess = true,
                        Data = package,
                        Messege = "Package retrived Successfully!"
                    });
                }
				return NotFound(new UserManagerResponse
				{
					IsSuccess = false,
					Messege = "Package not Found"
				});
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error while fetching packages data : {ex.Message}");
				return StatusCode(500, new UserManagerResponse
				{
					IsSuccess = false,
					Messege = "Something went wrong!"
				});
			}
		}
		[HttpPost("booking/initiate")]
		public async Task<IActionResult> InitiateBooking([FromBody] PackageBookingModel booking)
		{
			var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized(new { IsSuccess = false, Message = "Invalid Token!" });
			}

			if (booking == null || booking.NumberOfTravelers <= 0)
			{
				return BadRequest(new { IsSuccess = false, Message = "Invalid booking details!" });
			}

			var package = await _context.Packages.FindAsync(booking.PackageId);
			if (package == null || package.AvailableSpots < booking.NumberOfTravelers)
			{
				return BadRequest(new { IsSuccess = false, Message = "Invalid package or insufficient spots!" });
			}

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

			// Call Payment Service to create a Payment Intent
			var paymentResult = await _paymentService.CreatePaymentIntent(
				pendingBooking.Id,
				"Package",
				pendingBooking.TotalBill
			);

			// Access the `clientSecret` property correctly
			if (paymentResult is not null && paymentResult.GetType().GetProperty("clientSecret") != null)
			{
				var clientSecret = paymentResult.GetType().GetProperty("clientSecret")!.GetValue(paymentResult)?.ToString();

				return Ok(new { IsSuccess = true, BookingId = pendingBooking.Id, ClientSecret = clientSecret });
			}
			else
			{
				return BadRequest(new UserManagerResponse { IsSuccess = false, Messege = "Failed to create payment intent!" });
			}
		}
		//[HttpPost("booking/payment-success")]
		//public async Task<IActionResult> PaymentSuccess([FromBody] PaymentSuccessDto paymentData)
		//{
		//	if (paymentData == null || string.IsNullOrEmpty(paymentData.BookingId) || string.IsNullOrEmpty(paymentData.PaymentIntentId))
		//	{
		//		return BadRequest(new DataSendingResponse { IsSuccess = false, Message = "Invalid request data." });
		//	}
		//	var bookingId = Guid.Parse(paymentData.BookingId);

		//	var booking = await _context.UsersPackages.FindAsync(bookingId);

		//	if (booking == null)
		//	{
		//		return NotFound(new DataSendingResponse { IsSuccess = false, Message = "Booking not found." });
		//	}

		//	// Update booking status and store PaymentIntentId
		//	booking.Status = "Paid";  // Assuming "Paid" is the status for completed payments
		//	booking.PaymentIntentId = paymentData.PaymentIntentId;

		//	_context.UsersPackages.Update(booking);
		//	await _context.SaveChangesAsync();
		//	await _hubContext.Clients.All.SendAsync(
		//	"ReceiveNotification",
		//	$"✅ Payment successful! {booking.User.FullName} booked '{booking.Package.Title}' package. 💳 Payment ID: {paymentData.PaymentIntentId}"
		//	);

		//	return Ok(new DataSendingResponse { IsSuccess = true, Message = "Payment updated successfully." });
		//}
		[HttpPost("booking/payment-success")]
		public async Task<IActionResult> PaymentSuccess([FromBody] PaymentSuccessDto paymentData)
		{
			if (paymentData == null || string.IsNullOrEmpty(paymentData.BookingId) || string.IsNullOrEmpty(paymentData.PaymentIntentId))
			{
				return BadRequest(new DataSendingResponse { IsSuccess = false, Message = "Invalid request data." });
			}

			var bookingId = Guid.Parse(paymentData.BookingId);
			var booking = await _context.UsersPackages
										.Include(b => b.User)
										.Include(b => b.Package)
										.FirstOrDefaultAsync(b => b.Id == bookingId);

			if (booking == null)
			{
				return NotFound(new DataSendingResponse { IsSuccess = false, Message = "Booking not found." });
			}

			// ✅ Update booking status and store PaymentIntentId
			booking.Status = "Paid";
			booking.PaymentIntentId = paymentData.PaymentIntentId;

			_context.UsersPackages.Update(booking);
			await _context.SaveChangesAsync();

			// ✅ Send Real-time Notification (Admin)
			var adminIds = await _context.Users
										 .Where(u => u.Role == "Admin")
										 .Select(u => u.Id)
										 .ToListAsync();

			var notificationMessage = $"📝 {booking.User.FullName} booked '{booking.Package.Title}' successfully. 💳 Payment ID: {paymentData.PaymentIntentId}";
			foreach (var adminId in adminIds)
			{
				var adminNotification = new Notifications
				{
					UserId = adminId,
					Messege = notificationMessage
				};
				_context.Notifications.Add(adminNotification);

				// 🚀 Real-time notification
				await _hubContext.Clients.User(adminId.ToString())
								  .SendAsync("ReceiveNotification", adminNotification.Messege);
			}

			// 🎉 User Notification (Real-time)
			var userNotificationMessage = $"🎉 Your booking for '{booking.Package.Title}' is confirmed! 💳 Payment ID: {paymentData.PaymentIntentId}";
			var userNotification = new Notifications
			{
				UserId = booking.User.Id,
				Messege = userNotificationMessage
			};
			_context.Notifications.Add(userNotification);
			await _hubContext.Clients.User(booking.User.Id.ToString())
							  .SendAsync("ReceiveNotification", userNotificationMessage);

			await _context.SaveChangesAsync();
			var emailContent = EmailTemplates.PaymentSuccessTemplate(booking.User.FullName, booking.TotalBill.ToString(), DateTime.Now.ToString(), booking.Package.Title, booking.Package.Description, booking.Status, booking.PaymentIntentId);
			await _emailService.SendEmailAsync(booking.User.Email,"Booking SuccessFul",emailContent);

			return Ok(new DataSendingResponse { IsSuccess = true, Message = "Payment updated and latest notifications sent successfully." });
		}



	}
}
