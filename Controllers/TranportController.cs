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
using IdealTrip.Models.Tranport;

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

		public TransportController(ApplicationDbContext context, PaymentService paymentService, EmailService emailService, IHttpContextAccessor contextAccessor, IHubContext<NotificationHub> hubContext)
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
			if (!ModelState.IsValid)
			{
				return BadRequest(new UserManagerResponse
				{
					IsSuccess = false,
					Messege = "Invalid Data",
					Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
				});
			}
			var userId = _contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized(new DataSendingResponse { IsSuccess = false, Message = "Unauthorized action." });
			}
			try
			{
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
			catch (Exception ex)
			{
				return StatusCode(500, new DataSendingResponse
				{
					IsSuccess = false,
					Message = "An error occurred while fetching rooms.",
					Errors = new List<string> { ex.Message }
				});
			}
		}

		[HttpGet("get-all-transports")]
		[AllowAnonymous]
		public async Task<IActionResult> GetAllTransports()
		{
			try
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
			catch (Exception ex)
			{
				return StatusCode(500, new DataSendingResponse
				{
					IsSuccess = false,
					Message = "An error occurred while fetching rooms.",
					Errors = new List<string> { ex.Message }
				});
			}
		}
		[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		[Authorize]
		[HttpGet("get-transport/{id}")]
		public async Task<IActionResult> GetTransportById(Guid id)
		{
			try
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
			catch (Exception ex)
			{
				return StatusCode(500, new DataSendingResponse
				{
					IsSuccess = false,
					Message = "An error occurred while fetching rooms.",
					Errors = new List<string> { ex.Message }
				});
			}
		}


		[HttpGet("search-transports")]
		public async Task<IActionResult> SearchTransports([FromQuery] string? startLocation, [FromQuery] string? destination, [FromQuery] DateTime? departureDate)
		{
			try
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
			catch (Exception ex)
			{
				return StatusCode(500, new DataSendingResponse
				{
					IsSuccess = false,
					Message = "An error occurred while fetching rooms.",
					Errors = new List<string> { ex.Message }
				});
			}
		}
		[HttpPost("booking/initiate")]
		public async Task<IActionResult> InitiateBooking([FromBody] TransportBookingModel model)
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
			var userId = _contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized(new { IsSuccess = false, Message = "Invalid Token!" });
			}
			try
			{

				var transport = await _context.Transports.FindAsync(model.TransportId);

				if (!transport.IsAvailable)
					return BadRequest(new { IsSuccess = false, Message = "Invalid booking details or bus not available." });

				if (transport.SeatsAvailable < model.SeatsBooked)
					return BadRequest(new { IsSuccess = false, Message = "Not enough seats available." });
				UserTransportBooking booking = new();

				// Calculate Total Fare
				booking.TransportId = Guid.Parse(model.TransportId);
				booking.TicketPrice = transport.TicketPrice;
				booking.TotalFare = transport.TicketPrice * model.SeatsBooked;
				booking.Status = BookingStatus.Pending.ToString();
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
			catch (Exception ex)
			{
				return StatusCode(500, new DataSendingResponse
				{
					IsSuccess = false,
					Message = "An error occurred while fetching rooms.",
					Errors = new List<string> { ex.Message }
				});
			}
		}

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
					booking.Status = BookingStatus.Paid.ToString();
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
					string emailContent = EmailTemplates.TransportBookingSuccessTemplate(
	booking.User.FullName,
	booking.Transport.Name,
	booking.Transport.Type,
	booking.Transport.StartLocation,
	booking.Transport.Destination,
	booking.Transport.DepartureTime,
	booking.BookingDate,
	booking.SeatsBooked,
	booking.TotalFare.ToString("F2"),
	booking.PaymentIntentId,
	booking.Status
);

					await _emailService.SendEmailAsync(booking.User.Email, "🚍 Transport Booking Confirmed", emailContent);

					string ownerEmailContent = EmailTemplates.TransportBookingOwnerNotificationTemplate(
	booking.Transport.Owner.FullName,
	booking.User.FullName,
	booking.User.Email,
	booking.Transport.Name,
	booking.Transport.Type,
	booking.Transport.StartLocation,
	booking.Transport.Destination,
	booking.Transport.DepartureTime,
	booking.BookingDate,
	booking.SeatsBooked,
	booking.TotalFare.ToString("F2"),
	booking.PaymentIntentId
);

					await _emailService.SendEmailAsync(booking.Transport.Owner.Email, "🚍 Your Transport Just Got Booked!", ownerEmailContent);


					await transaction.CommitAsync();
					return Ok(new { IsSuccess = true, Message = "Payment updated, notifications sent successfully." });
				}
				catch (Exception ex)
				{
					await transaction.RollbackAsync();
					return BadRequest(new { IsSuccess = false, Message = "Payment processing failed." });
				}
			}
			catch (Exception ex)
			{
				return StatusCode(500, new DataSendingResponse
				{
					IsSuccess = false,
					Message = "An error occurred while fetching rooms.",
					Errors = new List<string> { ex.Message }
				});
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
			try
			{

				var transport = await _context.Transports.FindAsync(id);
				if (transport == null || transport.OwnerId != Guid.Parse(userId))
				{
					return NotFound(new { IsSuccess = false, Message = "Transport not found or unauthorized." });
				}

				_context.Transports.Remove(transport);
				await _context.SaveChangesAsync();

				return Ok(new { IsSuccess = true, Message = "Transport deleted successfully." });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new DataSendingResponse
				{
					IsSuccess = false,
					Message = "An error occurred while fetching rooms.",
					Errors = new List<string> { ex.Message }
				});
			}
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
			try
			{

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
			catch (Exception ex)
			{
				return StatusCode(500, new DataSendingResponse
				{
					IsSuccess = false,
					Message = "An error occurred while fetching rooms.",
					Errors = new List<string> { ex.Message }
				});
			}

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

			try
			{

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
			catch (Exception ex)
			{
				return StatusCode(500, new DataSendingResponse
				{
					IsSuccess = false,
					Message = "An error occurred while fetching rooms.",
					Errors = new List<string> { ex.Message }
				});
			}
		}
		[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		[Authorize(Roles = "Tourist")]
		[HttpPost("add-feedback")]
		public async Task<IActionResult> AddHotelFeedback([FromBody] FeedbackRequest request)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(new UserManagerResponse
				{
					IsSuccess = false,
					Messege = "Validation failed.",
					Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
				});
			}
			try
			{
				var userId = _contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
				if (string.IsNullOrEmpty(userId))
					return Unauthorized(new DataSendingResponse { IsSuccess = false, Message = "Unauthorized action." });

				var tranport = await _context.Transports.FirstOrDefaultAsync(h => h.Id == request.ServiceId);
				if (tranport == null)
					return NotFound(new DataSendingResponse { IsSuccess = false, Message = "Transport Not Found." });

				var feedback = new Feedback
				{
					Id = Guid.NewGuid(),
					UserId = Guid.Parse(userId),
					ServiceType = Service.Transport.ToString(),
					ServiceId = request.ServiceId,
					FeedbackText = request.FeedbackText,
					Rating = request.Rating,
					Date = DateTime.Now
				};

				_context.FeedBacks.Add(feedback);
				await _context.SaveChangesAsync();

				// Update hotel rating
				var avgRating = await _context.FeedBacks
					.Where(f => f.ServiceId == request.ServiceId && f.ServiceType == Service.Transport.ToString())
					.AverageAsync(f => (decimal?)f.Rating) ?? 0;

				tranport.Rating = (float)avgRating;
				_context.Transports.Update(tranport);
				await _context.SaveChangesAsync();

				return Ok(new DataSendingResponse { IsSuccess = true, Data = feedback, Message = "Feedback added successfully." });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new DataSendingResponse
				{
					IsSuccess = false,
					Message = "An error occurred while fetching rooms.",
					Errors = new List<string> { ex.Message }
				});
			}
		}


		[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		[Authorize(Roles = "Tourist")]
		[HttpGet("hotel/get-feedback/{hotelId}")]
		public async Task<IActionResult> GetHotelFeedbacks(Guid hotelId)
		{
			try
			{
				var feedbacks = await _context.FeedBacks
					.Where(f => f.ServiceId == hotelId && f.ServiceType == Service.Transport.ToString())
					.OrderByDescending(f => f.Date)
					.Select(f => new
					{
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
					Message = "Hotel feedback retrieved successfully."
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new DataSendingResponse
				{
					IsSuccess = false,
					Message = "An error occurred while fetching rooms.",
					Errors = new List<string> { ex.Message }
				});
			}
		}
	}
}
