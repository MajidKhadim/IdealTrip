using IdealTrip.Helpers;
using IdealTrip.Models.Tranport_Booking;
using IdealTrip.Models;
using IdealTrip.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using IdealTrip.Models.Package_Booking;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using IdealTrip.Models.Enums;
using Microsoft.EntityFrameworkCore;
using IdealTrip.Models.Database_Tables;
using Microsoft.AspNetCore.SignalR;

namespace IdealTrip.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class TransportController : ControllerBase
	{
		private readonly ApplicationDbContext _context;
		private readonly PaymentService _paymentService;
		private readonly EmailService _emailService;
		private readonly IHttpContextAccessor _contextAccessor;
		private readonly IHubContext<NotificationHub> _hubContext;

		public TransportController(ApplicationDbContext context, PaymentService paymentService, EmailService emailService, IHttpContextAccessor contextAccessor,IHubContext<NotificationHub> hubContext)
		{
			_context = context;
			_paymentService = paymentService;
			_emailService = emailService;
			_contextAccessor = contextAccessor;
			_hubContext = hubContext;
		}

		[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		[Authorize(Roles = "Transporter")]
		[HttpPost("add-transport")]
		public async Task<IActionResult> AddTransport([FromForm] AddTransportModel model)
		{
			var userId = _contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized(new DataSendingResponse { IsSuccess = false, Message = "Unauthorized action." });
			}

			var transportId = Guid.NewGuid();
			var transport = new Transport
			{
				Id = transportId,
				OwnerId = Guid.Parse(userId),
				Name = model.Name,
				Type = model.Type, // "Private" or "Bus"
				Capacity = model.Capacity,
				SeatsAvailable = model.Capacity, // Initial available seats same as capacity
				StartLocation = model.StartLocation,
				Destination = model.Destination,
				DepartureTime = model.DepartureTime,
				TicketPrice = model.PricePerSeat,
				CreatedAt = DateTime.Now,
				IsAvailable = true
			};

			_context.Transports.Add(transport);
			await _context.SaveChangesAsync();

			var imageUrls = new List<string>();
			string transportFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/TransportImages", transportId.ToString());

			if (!Directory.Exists(transportFolderPath))
			{
				Directory.CreateDirectory(transportFolderPath);
			}

			// Handle Primary Image
			if (model.PrimaryImage != null)
			{
				var primaryFileName = $"primary_{Guid.NewGuid()}_{Path.GetFileName(model.PrimaryImage.FileName)}";
				var primaryFilePath = Path.Combine(transportFolderPath, primaryFileName);

				using (var stream = new FileStream(primaryFilePath, FileMode.Create))
				{
					await model.PrimaryImage.CopyToAsync(stream);
				}

				var primaryImageUrl = $"/TransportImages/{transportId}/{primaryFileName}";
				imageUrls.Add(primaryImageUrl);

				_context.ServiceImages.Add(new ServiceImage
				{
					ServiceId = transport.Id,
					ServiceType = Service.Transport.ToString(),
					ImageUrl = primaryImageUrl,
					IsPrimary = true
				});
			}

			// Handle Additional Images
			if (model.Images != null && model.Images.Count > 0)
			{
				foreach (var image in model.Images)
				{
					var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(image.FileName)}";
					var filePath = Path.Combine(transportFolderPath, fileName);

					using (var stream = new FileStream(filePath, FileMode.Create))
					{
						await image.CopyToAsync(stream);
					}

					var imageUrl = $"/TransportImages/{transportId}/{fileName}";
					imageUrls.Add(imageUrl);

					_context.ServiceImages.Add(new ServiceImage
					{
						ServiceId = transport.Id,
						ServiceType = Service.Transport.ToString(),
						ImageUrl = imageUrl,
						IsPrimary = false
					});
				}
			}

			await _context.SaveChangesAsync();

			return Ok(new DataSendingResponse
			{
				IsSuccess = true,
				Data = new
				{
					transport,
					PrimaryImage = imageUrls.FirstOrDefault(),
					Images = imageUrls.Skip(1).ToList()
				},
				Message = "Transport added successfully with images."
			});
		}

		[HttpGet("get-all-transports")]
		public async Task<IActionResult> GetAllTransports()
		{
			var transports = await _context.Transports
				.Select(t => new
				{
					t.Id,
					t.Name,
					t.Type,
					t.Capacity,
					t.SeatsAvailable,
					t.StartLocation,
					t.Destination,
					t.DepartureTime,
					t.TicketPrice,
					t.IsAvailable,
					t.CreatedAt,
					PrimaryImage = _context.ServiceImages
						.Where(si => si.ServiceId == t.Id && si.ServiceType == Service.Transport.ToString() && si.IsPrimary)
						.Select(si => si.ImageUrl)
						.FirstOrDefault()
				})
				.ToListAsync();

			return Ok(new
			{
				IsSuccess = true,
				Data = transports,
				Message = "All transports retrieved successfully."
			});
		}

		[HttpGet("get-transport/{id}")]
		public async Task<IActionResult> GetTransportById(Guid id)
		{
			var transport = await _context.Transports
				.Where(t => t.Id == id)
				.Select(t => new
				{
					t.Id,
					t.Name,
					t.Type,
					t.Capacity,
					t.SeatsAvailable,
					t.StartLocation,
					t.Destination,
					t.DepartureTime,
					t.TicketPrice,
					t.IsAvailable,
					t.CreatedAt,
					t.OwnerId,
					OwnerName = t.Owner.UserName, // Assuming ApplicationUser has UserName
					Images = _context.ServiceImages
						.Where(si => si.ServiceId == t.Id && si.ServiceType == Service.Transport.ToString())
						.Select(si => new { si.ImageUrl, si.IsPrimary })
						.ToList()
				})
				.FirstOrDefaultAsync();

			if (transport == null)
				return NotFound(new { IsSuccess = false, Message = "Transport not found." });

			return Ok(new { IsSuccess = true, Data = transport, Message = "Transport details retrieved successfully." });
		}


		[HttpGet("search-transports")]
		public async Task<IActionResult> SearchTransports([FromQuery] string? startLocation, [FromQuery] string? destination, [FromQuery] DateTime? departureDate)
		{
			var query = _context.Transports.AsQueryable();

			if (!string.IsNullOrEmpty(startLocation))
				query = query.Where(t => t.StartLocation.Contains(startLocation));

			if (!string.IsNullOrEmpty(destination))
				query = query.Where(t => t.Destination.Contains(destination));

			if (departureDate.HasValue)
				query = query.Where(t => t.DepartureTime.Date == departureDate.Value.Date);

			var results = await query
				.Select(t => new
				{
					t.Id,
					t.Name,
					t.Type,
					t.Capacity,
					t.SeatsAvailable,
					t.StartLocation,
					t.Destination,
					t.DepartureTime,
					t.TicketPrice,
					t.IsAvailable,
					PrimaryImage = _context.ServiceImages
						.Where(si => si.ServiceId == t.Id && si.ServiceType == Service.Transport.ToString() && si.IsPrimary)
						.Select(si => si.ImageUrl)
						.FirstOrDefault()
				})
				.ToListAsync();

			return Ok(new
			{
				IsSuccess = true,
				Data = results,
				Message = "Transport search results retrieved successfully."
			});
		}
		[HttpPost("booking/initiate")]
		public async Task<IActionResult> InitiateBooking([FromBody] UserTransportBooking booking)
		{
			var userId = _contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized(new { IsSuccess = false, Message = "Invalid Token!" });
			}

			var transport = await _context.Transports.FindAsync(booking.TransportId);

			if (!transport.IsAvailable)
				return BadRequest(new { IsSuccess = false, Message = "Invalid booking details or bus not available." });

			if (transport.SeatsAvailable < booking.SeatsBooked)
				return BadRequest(new { IsSuccess = false, Message = "Not enough seats available." });

			// Calculate Total Fare
			booking.TicketPrice = transport.TicketPrice;
			booking.TotalFare = transport.TicketPrice * booking.SeatsBooked;
			booking.Status = "Pending";
			booking.BookingDate = DateTime.Now;
			booking.CreatedAt = DateTime.Now;
			booking.UserId = Guid.Parse(userId);

			_context.UserTransportBookings.Add(booking);
			await _context.SaveChangesAsync();

			// Create Payment Intent
			var paymentResult = await _paymentService.CreatePaymentIntent(booking.Id, "Bus", booking.TotalFare);
			var clientSecret = paymentResult?.GetType().GetProperty("clientSecret")?.GetValue(paymentResult)?.ToString();

			if (!string.IsNullOrEmpty(clientSecret))
			{
				return Ok(new { IsSuccess = true, BookingId = booking.Id, ClientSecret = clientSecret });
			}
			else
			{
				return BadRequest(new { IsSuccess = false, Message = "Failed to create payment intent!" });
			}
		}

		// 5️⃣ Confirm Payment & Update Booking
		//[HttpPost("booking/payment-success")]
		//public async Task<IActionResult> PaymentSuccess([FromBody] PaymentSuccessDto paymentData)
		//{
		//	if (paymentData == null || string.IsNullOrEmpty(paymentData.BookingId) || string.IsNullOrEmpty(paymentData.PaymentIntentId))
		//	{
		//		return BadRequest(new { IsSuccess = false, Message = "Invalid request data." });
		//	}

		//	var bookingId = Guid.Parse(paymentData.BookingId);
		//	var booking = await _context.UserTransportBookings
		//		.Include(b => b.User)
		//		.Include(b => b.Transport)
		//		.FirstOrDefaultAsync(b => b.Id == bookingId);

		//	if (booking == null)
		//	{
		//		return NotFound(new { IsSuccess = false, Message = "Booking not found." });
		//	}

		//	using var transaction = await _context.Database.BeginTransactionAsync();
		//	try
		//	{
		//		booking.Status = "Paid";
		//		booking.PaymentIntentId = paymentData.PaymentIntentId;
		//		_context.UserTransportBookings.Update(booking);

		//		// Deduct seats from transport
		//		var transport = await _context.Transports.FindAsync(booking.TransportId);
		//		if (transport != null)
		//		{
		//			transport.SeatsAvailable -= booking.SeatsBooked;
		//		}

		//		await _context.SaveChangesAsync();

		//		// Send Confirmation Email
		//		string content = EmailTemplates.PaymentSuccessTemplate(
		//			booking.User.FullName,
		//			booking.TotalFare.ToString(),
		//			booking.BookingDate.ToString(),
		//			booking.Transport.Name,
		//			booking.Transport.Type,
		//			booking.Status,
		//			booking.PaymentIntentId);

		//		await _emailService.SendEmailAsync(booking.User.Email, "Booking Successful", content);

		//		await transaction.CommitAsync();
		//		return Ok(new { IsSuccess = true, Message = "Payment updated successfully. Confirmation email sent." });
		//	}
		//	catch
		//	{
		//		await transaction.RollbackAsync();
		//		return BadRequest(new { IsSuccess = false, Message = "Payment processing failed." });
		//	}
		//}

		[HttpPost("booking/payment-success")]
		public async Task<IActionResult> PaymentSuccess([FromBody] PaymentSuccessDto paymentData)
		{
			if (paymentData == null || string.IsNullOrEmpty(paymentData.BookingId) || string.IsNullOrEmpty(paymentData.PaymentIntentId))
			{
				return BadRequest(new { IsSuccess = false, Message = "Invalid request data." });
			}

			var bookingId = Guid.Parse(paymentData.BookingId);
			var booking = await _context.UserTransportBookings
				.Include(b => b.User)
				.Include(b => b.Transport)
				.ThenInclude(t => t.Owner) // Ensure Transport Owner data is loaded
				.FirstOrDefaultAsync(b => b.Id == bookingId);

			if (booking == null)
			{
				return NotFound(new { IsSuccess = false, Message = "Booking not found." });
			}

			using var transaction = await _context.Database.BeginTransactionAsync();
			try
			{
				// ✅ Update booking status and store PaymentIntentId
				booking.Status = "Paid";
				booking.PaymentIntentId = paymentData.PaymentIntentId;
				_context.UserTransportBookings.Update(booking);

				// ✅ Deduct seats from transport
				var transport = await _context.Transports.FindAsync(booking.TransportId);
				if (transport != null)
				{
					transport.SeatsAvailable -= booking.SeatsBooked;
				}

				await _context.SaveChangesAsync();

				// ✅ Real-time Notification to Transport Owner
				var transportOwnerNotificationMessage = $"🚍 Your transport '{booking.Transport.Name}' has been booked by {booking.User.FullName} for {booking.BookingDate:MMMM dd, yyyy}. 🪑 Seats: {booking.SeatsBooked} 💳 Payment ID: {paymentData.PaymentIntentId}";
				var transportOwnerNotification = new Notifications
				{
					UserId = booking.Transport.Owner.Id,
					Messege = transportOwnerNotificationMessage
				};
				_context.Notifications.Add(transportOwnerNotification);
				await _hubContext.Clients.User(booking.Transport.Owner.Id.ToString())
								  .SendAsync("ReceiveNotification", transportOwnerNotificationMessage);

				// ✅ Real-time Notification to User
				var userNotificationMessage = $"🎉 Your transport booking for '{booking.Transport.Name}' is confirmed for {booking.BookingDate:MMMM dd, yyyy}! 🪑 Seats: {booking.SeatsBooked} 💳 Payment ID: {paymentData.PaymentIntentId}";
				var userNotification = new Notifications
				{
					UserId = booking.User.Id,
					Messege = userNotificationMessage
				};
				_context.Notifications.Add(userNotification);
				await _hubContext.Clients.User(booking.User.Id.ToString())
								  .SendAsync("ReceiveNotification", userNotificationMessage);

				// ✅ Notify All Users (Optional)
				await _hubContext.Clients.All.SendAsync("ReceiveNotification",
					$"📢 {booking.User.FullName} just booked {booking.SeatsBooked} seats on transport '{booking.Transport.Name}' for {booking.BookingDate:MMMM dd, yyyy}!");

				await _context.SaveChangesAsync();

				// ✅ Send Email Confirmation
				string emailContent = EmailTemplates.PaymentSuccessTemplate(
					booking.User.FullName,
					booking.TotalFare.ToString(),
					booking.BookingDate.ToString("MMMM dd, yyyy"),
					booking.Transport.Name,
					booking.Transport.Type,
					booking.Status,
					booking.PaymentIntentId
				);

				await _emailService.SendEmailAsync(booking.User.Email, "Booking Successful", emailContent);

				await transaction.CommitAsync();
				return Ok(new { IsSuccess = true, Message = "Payment updated, notifications sent successfully." });
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				return BadRequest(new { IsSuccess = false, Message = "Payment processing failed."});
			}
		}


		//[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		//[Authorize(Roles = "Transporter")]
		//[HttpPut("update-transport/{id}")]
		//public async Task<IActionResult> UpdateTransport(Guid id, [FromBody] UpdateTransportModel model)
		//{
		//	var userId = _contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
		//	if (string.IsNullOrEmpty(userId))
		//	{
		//		return Unauthorized(new { IsSuccess = false, Message = "Unauthorized action." });
		//	}

		//	var transport = await _context.Transports.FindAsync(id);
		//	if (transport == null || transport.OwnerId != Guid.Parse(userId))
		//	{
		//		return NotFound(new { IsSuccess = false, Message = "Transport not found or unauthorized." });
		//	}

		//	transport.Name = model.Name ?? transport.Name;
		//	transport.Type = model.Type ?? transport.Type;
		//	transport.Capacity = model.Capacity ?? transport.Capacity;
		//	transport.SeatsAvailable = model.SeatsAvailable ?? transport.SeatsAvailable;
		//	transport.StartLocation = model.StartLocation ?? transport.StartLocation;
		//	transport.Destination = model.Destination ?? transport.Destination;
		//	transport.DepartureTime = model.DepartureTime ?? transport.DepartureTime;
		//	transport.TicketPrice = model.TicketPrice ?? transport.TicketPrice;
		//	transport.IsAvailable = model.IsAvailable ?? transport.IsAvailable;

		//	_context.Transports.Update(transport);
		//	await _context.SaveChangesAsync();

		//	return Ok(new { IsSuccess = true, Message = "Transport updated successfully." });
		//}

		[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		[Authorize(Roles = "Transporter")]
		[HttpDelete("delete-transport/{id}")]
		public async Task<IActionResult> DeleteTransport(Guid id)
		{
			var userId = _contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized(new { IsSuccess = false, Message = "Unauthorized action." });
			}

			var transport = await _context.Transports.FindAsync(id);
			if (transport == null || transport.OwnerId != Guid.Parse(userId))
			{
				return NotFound(new { IsSuccess = false, Message = "Transport not found or unauthorized." });
			}

			_context.Transports.Remove(transport);
			await _context.SaveChangesAsync();

			return Ok(new { IsSuccess = true, Message = "Transport deleted successfully." });
		}

		[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		[HttpGet("user-bookings")]
		public async Task<IActionResult> GetUserBookings()
		{
			var userId = _contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized(new { IsSuccess = false, Message = "Unauthorized action." });
			}

			var bookings = await _context.UserTransportBookings
				.Where(b => b.UserId == Guid.Parse(userId))
				.Select(b => new
				{
					b.Id,
					b.TransportId,
					b.SeatsBooked,
					b.TicketPrice,
					b.TotalFare,
					b.Status,
					b.BookingDate,
					TransportName = b.Transport.Name,
					StartLocation = b.Transport.StartLocation,
					Destination = b.Transport.Destination,
					DepartureTime = b.Transport.DepartureTime
				})
				.ToListAsync();

			return Ok(new { IsSuccess = true, Data = bookings, Message = "User bookings retrieved successfully." });
		}

		//[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		//[HttpPost("cancel-booking/{id}")]
		//public async Task<IActionResult> CancelBooking(Guid id)
		//{
		//	var userId = _contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
		//	if (string.IsNullOrEmpty(userId))
		//	{
		//		return Unauthorized(new { IsSuccess = false, Message = "Unauthorized action." });
		//	}

		//	var booking = await _context.UserTransportBookings.FindAsync(id);
		//	if (booking == null || booking.UserId != Guid.Parse(userId))
		//	{
		//		return NotFound(new { IsSuccess = false, Message = "Booking not found or unauthorized." });
		//	}

		//	if (booking.Status == "Paid")
		//	{
		//		return BadRequest(new { IsSuccess = false, Message = "Paid bookings cannot be cancelled." });
		//	}

		//	booking.Status = "Cancelled";
		//	_context.UserTransportBookings.Update(booking);
		//	await _context.SaveChangesAsync();

		//	return Ok(new { IsSuccess = true, Message = "Booking cancelled successfully." });
		//}

		[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		[Authorize(Roles = "Transporter")]
		[HttpGet("transport-bookings/{transportId}")]
		public async Task<IActionResult> GetTransportBookings(Guid transportId)
		{
			var userId = _contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized(new { IsSuccess = false, Message = "Unauthorized action." });
			}

			var transport = await _context.Transports.FindAsync(transportId);
			if (transport == null || transport.OwnerId != Guid.Parse(userId))
			{
				return NotFound(new { IsSuccess = false, Message = "Transport not found or unauthorized." });
			}

			var bookings = await _context.UserTransportBookings
				.Where(b => b.TransportId == transportId)
				.Select(b => new
				{
					b.Id,
					b.UserId,
					UserName = b.User.FullName,
					b.SeatsBooked,
					b.TotalFare,
					b.Status,
					b.BookingDate
				})
				.ToListAsync();

			return Ok(new { IsSuccess = true, Data = bookings, Message = "Transport bookings retrieved successfully." });
		}
	}
}
