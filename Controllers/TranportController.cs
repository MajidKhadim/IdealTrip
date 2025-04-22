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
			if (model.DepartureTime <= DateTime.Now)
			{
				return BadRequest(new DataSendingResponse { IsSuccess = false, Message = "Kindly Add a Valid and Upcoming Departure Time" });
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
					IsAvailable = true,
					IsDeleted = false,
					UpdatedAt = DateTime.Now
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
					Message = "Internal Server Error",
					Errors = new List<string> { ex.Message }
				});
			}
		}
		[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		[Authorize(Roles = "Transporter")]
		[HttpPut("update-transport/{transportId}")]
		public async Task<IActionResult> UpdateTransport(string transportId,[FromForm] AddTransportModel model)
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

			var transport = await _context.Transports.FindAsync(Guid.Parse(transportId));
			if (transport == null || transport.IsDeleted)
			{
				return NotFound(new DataSendingResponse { IsSuccess = false, Message = "Transport not found." });
			}

			if (transport.OwnerId.ToString() != userId)
			{
				return Forbid();
			}

			if (model.DepartureTime <= DateTime.Now)
			{
				return BadRequest(new DataSendingResponse { IsSuccess = false, Message = "Please provide a valid future departure time." });
			}

			try
			{
				// Update basic transport fields
				transport.Name = model.Name;
				transport.Type = model.Type;
				transport.Capacity = model.Capacity;
				transport.SeatsAvailable = model.Capacity;
				transport.StartLocation = model.StartLocation;
				transport.Destination = model.Destination;
				transport.DepartureTime = model.DepartureTime;
				transport.TicketPrice = model.PricePerSeat;
				transport.UpdatedAt = DateTime.Now;

				string transportFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/TransportImages", transportId.ToString());
				if (!Directory.Exists(transportFolderPath))
					Directory.CreateDirectory(transportFolderPath);

				var imageUrls = new List<string>();

				// === Delete old primary image if new one is uploaded ===
				if (model.PrimaryImage != null)
				{
					var oldPrimary = await _context.ServiceImages
						.FirstOrDefaultAsync(i => i.ServiceId.ToString() == transportId && i.ServiceType == Service.Transport.ToString() && i.IsPrimary);

					if (oldPrimary != null)
					{
						string oldPrimaryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", oldPrimary.ImageUrl.TrimStart('/'));
						if (System.IO.File.Exists(oldPrimaryPath))
							System.IO.File.Delete(oldPrimaryPath);

						_context.ServiceImages.Remove(oldPrimary);
					}

					var primaryFileName = $"primary_{Guid.NewGuid()}_{Path.GetFileName(model.PrimaryImage.FileName)}";
					var primaryFilePath = Path.Combine(transportFolderPath, primaryFileName);
					using (var stream = new FileStream(primaryFilePath, FileMode.Create))
						await model.PrimaryImage.CopyToAsync(stream);

					var primaryImageUrl = $"/TransportImages/{transportId}/{primaryFileName}";
					imageUrls.Add(primaryImageUrl);

					_context.ServiceImages.Add(new ServiceImage
					{
						ServiceId = Guid.Parse(transportId),
						ServiceType = Service.Transport.ToString(),
						ImageUrl = primaryImageUrl,
						IsPrimary = true
					});
				}

				// === Delete all old non-primary images if new ones are uploaded ===
				if (model.Images != null && model.Images.Count > 0)
				{
					var oldNonPrimaryImages = await _context.ServiceImages
						.Where(i => i.ServiceId.ToString() == transportId && i.ServiceType == Service.Transport.ToString() && !i.IsPrimary)
						.ToListAsync();

					foreach (var img in oldNonPrimaryImages)
					{
						string imgPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", img.ImageUrl.TrimStart('/'));
						if (System.IO.File.Exists(imgPath))
							System.IO.File.Delete(imgPath);

						_context.ServiceImages.Remove(img);
					}

					// Add new images
					foreach (var image in model.Images)
					{
						var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(image.FileName)}";
						var filePath = Path.Combine(transportFolderPath, fileName);

						using (var stream = new FileStream(filePath, FileMode.Create))
							await image.CopyToAsync(stream);

						var imageUrl = $"/TransportImages/{transportId}/{fileName}";
						imageUrls.Add(imageUrl);

						_context.ServiceImages.Add(new ServiceImage
						{
							ServiceId = Guid.Parse(transportId),
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
					Message = "Transport updated successfully.",
					Data = new
					{
						transport,
						PrimaryImage = imageUrls.FirstOrDefault(),
						Images = imageUrls.Skip(1).ToList()
					}
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

		[AllowAnonymous]
		[HttpGet("get-all-transports")]
		
		public async Task<IActionResult> GetAllTransports()
		{
			try
			{
				var transports = await _context.Transports
					.Where(t => t.DepartureTime > DateTime.Now && !t.IsDeleted)
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
					}).Where(t => t.DepartureTime > DateTime.Now )
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
					Message = "Internal Server Error",
					Errors = new List<string> { ex.Message }
				});
			}
		}
		[Authorize(AuthenticationSchemes =JwtBearerDefaults.AuthenticationScheme)]
		[Authorize(Roles =("Transporter"))]
		[HttpGet("my-tranports")]

		public async Task<IActionResult> GetMyTransports()
		{
			try
			{
				var userId = _contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
				if (string.IsNullOrEmpty(userId))
				{
					return Unauthorized(new DataSendingResponse { IsSuccess = false, Message = "Unauthorized action." });
				}
				var tranports = await _context.Transports.Where(t => t.OwnerId.ToString() == userId && !t.IsDeleted)
					.Select(t => new
					{
						t.Id,
						t.Name,
						t.Type,
						t.StartLocation,
						t.Destination,
						t.DepartureTime,
						t.TicketPrice,
					}
					).ToListAsync();
				return Ok(new UserManagerResponse
				{
					IsSuccess = true,
					Messege = "Tranports Retrived successfully",
					Data = tranports
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
		[AllowAnonymous]
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
					Message = "Internal Server Error",
					Errors = new List<string> { ex.Message }
				});
			}
		}

		[AllowAnonymous]
		[HttpGet("search-transports")]
		public async Task<IActionResult> SearchTransports([FromQuery] string? startLocation, [FromQuery] string? destination, [FromQuery] DateTime? departureDate)
		{
			try
			{
				var query = _context.Transports.AsQueryable();

				if (!string.IsNullOrEmpty(startLocation))
					query = query.Where(t => t.StartLocation.Contains(startLocation) && !t.IsDeleted);

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
					}).Where(t => t.DepartureTime <  DateTime.Now)
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
					Message = "Internal Server Error",
					Errors = new List<string> { ex.Message }
				});
			}
		}
		[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		[Authorize(Roles =("Tourist"))]
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

				var transport = await _context.Transports.FindAsync(Guid.Parse(model.TransportId));

				if (!transport.IsAvailable)
					return BadRequest(new { IsSuccess = false, Message = "Invalid booking details or bus not available." });
				if(transport.DepartureTime < DateTime.Now)
				{
					return BadRequest(new { IsSuccess = false, Message = "The Transport has gone" });
				}
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
				booking.SeatsBooked = model.SeatsBooked;
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
					Message = "An error occurred while booking",
					Errors = new List<string> { ex.Message }
				});
			}
		}
		[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		[Authorize(Roles = ("Tourist"))]
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
					Message = "Internal Server Error",
					Errors = new List<string> { ex.Message }
				});
			}
		}


		

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
				transport.IsDeleted = true;
				_context.Update(transport);
				await _context.SaveChangesAsync();

				return Ok(new { IsSuccess = true, Message = "Transport deleted successfully." });
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
		[Authorize(Roles =("Tourist"))]
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
					Message = "Internal Server Error",
					Errors = new List<string> { ex.Message }
				});
			}

		}

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
					Message = "Internal Server Error",
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
					Message = "An error occurred while adding feedbacks.",
					Errors = new List<string> { ex.Message }
				});
			}
		}


		[HttpGet("get-feedback/{tranportId}")]
		public async Task<IActionResult> GetHotelFeedbacks(Guid transportId)
		{
			try
			{
				var feedbacks = await _context.FeedBacks
					.Where(f => f.ServiceId == transportId && f.ServiceType == Service.Transport.ToString())
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
					Message = "An error occurred while fetching feedbacks.",
					Errors = new List<string> { ex.Message }
				});
			}
		}
	}
}
