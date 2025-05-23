﻿using IdealTrip.Helpers;
using IdealTrip.Models;
using IdealTrip.Models.Database_Tables;
using IdealTrip.Models.Enums;
using IdealTrip.Models.LocalHome_Booking;
using IdealTrip.Models.Package_Booking;
using IdealTrip.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Stripe.Terminal;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdealTrip.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class LocalHomeController : ControllerBase
	{
		private readonly ApplicationDbContext _context;
		private readonly IHttpContextAccessor _contextAccessor;
		private readonly PaymentService _paymentService;
		private readonly EmailService _emailService;
		private readonly IHubContext<NotificationHub> _hubContext;

		public LocalHomeController(ApplicationDbContext context, IHttpContextAccessor contextAccessor, PaymentService paymentService, IHubContext<NotificationHub> hubContext,EmailService emailService)
		{
			_context = context;
			_contextAccessor = contextAccessor;
			_paymentService = paymentService;
			_hubContext = hubContext;
			_emailService = emailService;
		}

		[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		[Authorize(Roles = "LocalHomeOwner")]
		[HttpPost("add-localhome")]
		public async Task<IActionResult> AddLocalHome([FromForm] AddLocalHomeModel model)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(new UserManagerResponse
				{
					IsSuccess = false,
					Messege = "Some Properties are not Valid",
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
				if (model.AvailableFrom >= model.AvailableTo)
				{
					return BadRequest(new DataSendingResponse { IsSuccess = false, Message = "AvalibleFrom date must be before AvalibleTo." });
				}

				var localHomeId = Guid.NewGuid();
				var localHome = new LocalHome
				{
					Id = localHomeId,
					OwnerId = Guid.Parse(userId),
					Name = model.Name,
					Description = model.Description,
					AddressLine = model.AddressLine,
					PricePerNight = model.PricePerNight,
					Capacity = model.Capacity,
					AvailableFrom = model.AvailableFrom,
					AvailableTo = model.AvailableTo,
					CreatedAt = DateTime.Now,
					UpdatedAt = DateTime.Now,
					IsAvailable = true,
					NumberOfRooms = model.NumberOfRooms,
					Rating = 0
				};

				_context.LocalHomes.Add(localHome);
				await _context.SaveChangesAsync();

				var imageUrls = new List<string>();
				string homeFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/PropertyImages/LocalHomes", localHomeId.ToString());

				if (!Directory.Exists(homeFolderPath))
				{
					Directory.CreateDirectory(homeFolderPath);
				}

				// Handle Primary Image
				if (model.PrimaryImage != null)
				{
					var primaryFileName = $"primary_{Guid.NewGuid()}_{Path.GetFileName(model.PrimaryImage.FileName)}";
					var primaryFilePath = Path.Combine(homeFolderPath, primaryFileName);

					using (var stream = new FileStream(primaryFilePath, FileMode.Create))
					{
						await model.PrimaryImage.CopyToAsync(stream);
					}

					var primaryImageUrl = $"/PropertyImages/LocalHomes/{localHomeId}/{primaryFileName}";
					imageUrls.Add(primaryImageUrl);

					_context.ServiceImages.Add(new ServiceImage
					{
						ServiceId = localHome.Id,
						ServiceType = Service.LocalHome.ToString(),
						ImageUrl = primaryImageUrl,
						IsPrimary = true // Mark this as the primary image
					});
				}

				// Handle Additional Images
				if (model.Images != null && model.Images.Count > 0)
				{
					foreach (var image in model.Images)
					{
						var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(image.FileName)}";
						var filePath = Path.Combine(homeFolderPath, fileName);

						using (var stream = new FileStream(filePath, FileMode.Create))
						{
							await image.CopyToAsync(stream);
						}

						var imageUrl = $"/PropertyImages/LocalHomes/{localHomeId}/{fileName}";
						imageUrls.Add(imageUrl);

						_context.ServiceImages.Add(new ServiceImage
						{
							ServiceId = localHome.Id,
							ServiceType = Service.LocalHome.ToString(),
							ImageUrl = imageUrl,
							IsPrimary = false // Additional images
						});
					}
				}

				await _context.SaveChangesAsync();

				return Ok(new DataSendingResponse
				{
					IsSuccess = true,
					Data = new { localHome, PrimaryImage = imageUrls.FirstOrDefault(), Images = imageUrls.Skip(1).ToList() },
					Message = "Local home added successfully with images."
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
		[Authorize(Roles = "LocalHomeOwner")]
		[HttpPut("update-localhome/{id}")]
		public async Task<IActionResult> UpdateLocalHome(Guid id, [FromForm] AddLocalHomeModel updatedHome)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(new UserManagerResponse
				{
					IsSuccess = false,
					Messege = "Some Properties are not Valid",
					Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
				});
			}
			try
			{
				var userId = _contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
				var home = await _context.LocalHomes.FirstOrDefaultAsync(h => h.Id == id && !h.IsDeleted);
				if (home == null)
				{
					return NotFound(new DataSendingResponse { IsSuccess = false, Message = "Local home not found." });
				}
				if (userId != home.OwnerId.ToString())
				{
					return Forbid();
				}
				if (updatedHome.AvailableFrom >= updatedHome.AvailableTo)
				{
					return BadRequest(new DataSendingResponse { IsSuccess = false, Message = "AvailableFrom date must be before AvailableTo." });
				}

				// ✅ Update Home Details
				home.Name = updatedHome.Name;
				home.Description = updatedHome.Description;
				home.AddressLine = updatedHome.AddressLine;
				home.PricePerNight = updatedHome.PricePerNight;
				home.Capacity = updatedHome.Capacity;
				home.AvailableFrom = updatedHome.AvailableFrom;
				home.AvailableTo = updatedHome.AvailableTo;
				home.UpdatedAt = DateTime.Now;
				home.NumberOfRooms = updatedHome.NumberOfRooms;

				await _context.SaveChangesAsync(); // Save home details before updating images

				string homeFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/PropertyImages/LocalHomes", home.Id.ToString());

				if (!Directory.Exists(homeFolderPath))
				{
					Directory.CreateDirectory(homeFolderPath);
				}

				// ✅ Fetch Images from Separate ServiceImages Table
				var serviceImages = await _context.ServiceImages.Where(img => img.ServiceId == home.Id && img.ServiceType == Service.LocalHome.ToString()).ToListAsync();
				var primaryImage = serviceImages.FirstOrDefault(img => img.IsPrimary);
				var additionalImages = serviceImages.Where(img => !img.IsPrimary).ToList();

				// ✅ Handle Primary Image Update
				if (updatedHome.PrimaryImage != null)
				{
					if (primaryImage != null)
					{
						string oldPrimaryImagePath = Path.Combine(homeFolderPath, Path.GetFileName(primaryImage.ImageUrl));
						if (System.IO.File.Exists(oldPrimaryImagePath))
						{
							System.IO.File.Delete(oldPrimaryImagePath);
						}
						_context.ServiceImages.Remove(primaryImage);
					}

					var primaryFileName = $"primary_{Guid.NewGuid()}_{Path.GetFileName(updatedHome.PrimaryImage.FileName)}";
					var primaryFilePath = Path.Combine(homeFolderPath, primaryFileName);

					using (var stream = new FileStream(primaryFilePath, FileMode.Create))
					{
						await updatedHome.PrimaryImage.CopyToAsync(stream);
					}

					var primaryImageUrl = $"/PropertyImages/LocalHomes/{home.Id}/{primaryFileName}";

					_context.ServiceImages.Add(new ServiceImage
					{
						Id = Guid.NewGuid(),
						ServiceId = home.Id,
						ServiceType = Service.LocalHome.ToString(),
						ImageUrl = primaryImageUrl,
						IsPrimary = true
					});
				}

				// ✅ Handle Additional Images Update
				if (updatedHome.Images != null && updatedHome.Images.Count > 0)
				{
					// Delete only if new images are provided
					foreach (var img in additionalImages)
					{
						string oldImagePath = Path.Combine(homeFolderPath, Path.GetFileName(img.ImageUrl));
						if (System.IO.File.Exists(oldImagePath))
						{
							System.IO.File.Delete(oldImagePath);
						}
						_context.ServiceImages.Remove(img);
					}

					foreach (var image in updatedHome.Images)
					{
						var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(image.FileName)}";
						var filePath = Path.Combine(homeFolderPath, fileName);

						using (var stream = new FileStream(filePath, FileMode.Create))
						{
							await image.CopyToAsync(stream);
						}

						var imageUrl = $"/PropertyImages/LocalHomes/{home.Id}/{fileName}";

						_context.ServiceImages.Add(new ServiceImage
						{
							Id = Guid.NewGuid(),
							ServiceId = home.Id,
							ServiceType = Service.LocalHome.ToString(),
							ImageUrl = imageUrl,
							IsPrimary = false
						});
					}
				}

				await _context.SaveChangesAsync();

				return Ok(new DataSendingResponse
				{
					IsSuccess = true,
					Message = "Local home updated successfully."
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
		[Authorize(Roles = "LocalHomeOwner")]
		[HttpDelete("delete-localhome/{id}")]
		public async Task<IActionResult> DeleteLocalHome(Guid id)
		{
			try
			{
				var userId = _contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
				var home = await _context.LocalHomes.FirstOrDefaultAsync(h => h.Id == id && !h.IsDeleted);
				if (home == null)
				{
					return NotFound(new DataSendingResponse { IsSuccess = false, Message = "Local home not found." });
				}
				if (userId != home.OwnerId.ToString())
				{
					return Forbid();
				}
				home.IsDeleted = true;
				_context.Update(home);
				await _context.SaveChangesAsync();

				return Ok(new DataSendingResponse
				{
					IsSuccess = true,
					Message = "Local home and associated images deleted successfully."
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
		[HttpGet("GetLocalHomes")]
		public async Task<IActionResult> GetLocalHomes(
	[FromQuery] string? location = null,
	[FromQuery] decimal? minPrice = null,
	[FromQuery] decimal? maxPrice = null,
	[FromQuery] int? minCapacity = null,
	[FromQuery] int? numberOfRooms = null,
	[FromQuery] DateOnly? startDate = null,
	[FromQuery] DateOnly? endDate = null)
		{
			try
			{
				var query = _context.LocalHomes.Where(h => h.IsAvailable && !h.IsDeleted && !h.Owner.IsDeleted);

				// Location filter
				if (!string.IsNullOrEmpty(location))
					query = query.Where(h => h.AddressLine.Contains(location));

				// Price range filter
				if (minPrice.HasValue)
					query = query.Where(h => h.PricePerNight >= minPrice.Value);
				if (maxPrice.HasValue)
					query = query.Where(h => h.PricePerNight <= maxPrice.Value);

				// Capacity filter
				if (minCapacity.HasValue)
					query = query.Where(h => h.Capacity >= minCapacity.Value);

				// Number of rooms filter
				if (numberOfRooms.HasValue)
					query = query.Where(h => h.NumberOfRooms >= numberOfRooms.Value);
				if (startDate >= endDate)
				{
					return BadRequest(new DataSendingResponse { IsSuccess = false, Message = "start date must be before End Date." });
				}

				// Date availability filter
				if (startDate.HasValue && endDate.HasValue)
				{
					query = query.Where(h =>
						h.AvailableFrom <= startDate.Value &&  // Home available on/before start
						h.AvailableTo >= endDate.Value         // Home available on/after end
					);
				}
				else if (startDate.HasValue)
				{
					query = query.Where(h => h.AvailableTo >= startDate.Value);
				}
				else if (endDate.HasValue)
				{
					query = query.Where(h => h.AvailableFrom <= endDate.Value);
				}

				var localHomes = await query.Select(lh => new
				{
					lh.Id,
					lh.Name,
					lh.Description,
					lh.AddressLine,
					lh.AvailableFrom,
					lh.AvailableTo,
					lh.Capacity,
					lh.PricePerNight,
					lh.Rating,
					ImageUrl = _context.ServiceImages
						.Where(img => img.ServiceId == lh.Id &&
									 img.ServiceType == Service.LocalHome.ToString() &&
									 img.IsPrimary)
						.Select(img => img.ImageUrl)
						.FirstOrDefault()
				}).ToListAsync();

				return Ok(new UserManagerResponse
				{
					Messege = "Local homes retrieved successfully",
					IsSuccess = true,
					Data = new
					{
						TotalRecords = localHomes.Count,
						LocalHomes = localHomes
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
		[HttpGet("GetLocalHomeById/{id}")]
		public async Task<IActionResult> GetLocalHomeById(Guid id)
		{
			try
			{
				var localHome = await _context.LocalHomes
					.Where(h => h.Id == id && h.IsAvailable && !h.IsDeleted && !h.Owner.IsDeleted)
					.Select(lh => new
					{
						lh.Id,
						lh.Name,
						lh.Description,
						lh.AddressLine,
						lh.AvailableFrom,
						lh.AvailableTo,
						lh.Capacity,
						lh.PricePerNight,
						lh.Rating,
						lh.NumberOfRooms,
						MainImage = _context.ServiceImages
							.Where(img => img.ServiceId == lh.Id && img.ServiceType == Service.LocalHome.ToString() && img.IsPrimary)
							.Select(img => img.ImageUrl)
							.FirstOrDefault(),
						Images = _context.ServiceImages
							.Where(img => img.ServiceId == lh.Id && img.ServiceType == Service.LocalHome.ToString() && img.IsPrimary != true)
							.Select(img => img.ImageUrl)
							.ToList()
					})
					.FirstOrDefaultAsync();

				if (localHome == null)
				{
					return NotFound(new DataSendingResponse { IsSuccess = false, Message = "Local home not found." });
				}

				return Ok(new DataSendingResponse
				{
					IsSuccess = true,
					Data = localHome,
					Message = "Local home retrieved successfully."
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
		[Authorize(Roles = "Tourist")]
		[HttpPost("booking/initiate")]
		public async Task<IActionResult> InitiateBooking([FromBody] LocalHomeBookingModel booking)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(new UserManagerResponse
				{
					IsSuccess = false,
					Messege = "Invalid Booking Data",
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

				if (booking == null || booking.TotalDays <= 0 || booking.StartDate == null || booking.EndDate == null)
				{
					return BadRequest(new { IsSuccess = false, Message = "Invalid booking details!" });
				}
				if (booking.StartDate >= booking.EndDate)
				{
					return BadRequest(new DataSendingResponse { IsSuccess = false, Message = "Start date must be before End Date." });
				}

				var localHome = await _context.LocalHomes.FindAsync(booking.LocalHomeId);
				if (localHome == null || !localHome.IsAvailable)
				{
					return BadRequest(new { IsSuccess = false, Message = "Local home not available!" });
				}

				// Check if the home is available for the requested date range
				var existingBookings = await _context.UserLocalHomesBookings
					.Where(b => b.LocalHomeId == booking.LocalHomeId && b.Status == BookingStatus.Paid.ToString())
					.ToListAsync();

				foreach (var existingBooking in existingBookings)
				{
					// Check if the booking period overlaps
					if ((booking.StartDate >= existingBooking.StartDate && booking.StartDate <= existingBooking.EndDate) ||
						(booking.EndDate >= existingBooking.StartDate && booking.EndDate <= existingBooking.EndDate) ||
						(booking.StartDate <= existingBooking.StartDate && booking.EndDate >= existingBooking.EndDate))
					{
						return BadRequest(new { IsSuccess = false, Message = "Local home is already booked for the selected dates!" });
					}
				}

				// Calculate total cost
				decimal totalCost = booking.TotalDays * localHome.PricePerNight;

				var pendingBooking = new UserLocalHomeBooking
				{
					UserId = Guid.Parse(userId),
					LocalHomeId = booking.LocalHomeId,
					TotalAmount = totalCost,
					Status = "Pending",
					StartDate = booking.StartDate,   // Assuming StartDate and EndDate are part of LocalHomeBookingModel
					EndDate = booking.EndDate,
					TotalDays = booking.TotalDays
				};

				_context.UserLocalHomesBookings.Add(pendingBooking);
				await _context.SaveChangesAsync();

				var paymentResult = await _paymentService.CreatePaymentIntent(pendingBooking.Id, "LocalHome", pendingBooking.TotalAmount);
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
					Messege = "Validation failed.",
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
				var booking = await _context.UserLocalHomesBookings
											.Include(b => b.User)
											.Include(b => b.LocalHome)
											.ThenInclude(lh => lh.Owner)  // Ensure Home Owner data is loaded
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
					_context.UserLocalHomesBookings.Update(booking);
					await _context.SaveChangesAsync();

					// ✅ Real-time Notification to Home Owner
					var homeOwnerNotificationMessage = $"🏠 Your local home '{booking.LocalHome.Name}' has been booked by {booking.User.FullName} for {booking.BookingDate:MMMM dd, yyyy}. 💳 Payment ID: {paymentData.PaymentIntentId}";
					var homeOwnerNotification = new Notifications
					{
						UserId = booking.LocalHome.Owner.Id,
						Messege = homeOwnerNotificationMessage
					};
					_context.Notifications.Add(homeOwnerNotification);
					await _hubContext.Clients.User(booking.LocalHome.Owner.Id.ToString())
									  .SendAsync("ReceiveNotification", homeOwnerNotificationMessage);

					// ✅ Real-time Notification to User
					var userNotificationMessage = $"🎉 Your booking for '{booking.LocalHome.Name}' is confirmed for {booking.BookingDate:MMMM dd, yyyy}! 💳 Payment ID: {paymentData.PaymentIntentId}";
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
						$"📢 {booking.User.FullName} just booked a local home '{booking.LocalHome.Name}' for {booking.BookingDate:MMMM dd, yyyy}!");

					await _context.SaveChangesAsync();

					// ✅ Send Email Confirmation
					string emailContent = EmailTemplates.LocalHomeBookingSuccessTemplate(
	booking.User.FullName,
	booking.LocalHome.Name,
	booking.LocalHome.Description,
	booking.StartDate,
	booking.EndDate,
	booking.TotalDays,
	booking.TotalAmount.ToString("F2"),
	booking.PaymentIntentId,
	booking.Status,
	booking.BookingDate
);

					await _emailService.SendEmailAsync(booking.User.Email, "🏠 Local Home Booking Confirmed", emailContent);
					// ✅ Send Email to Local Home Owner
					string ownerEmailContent = EmailTemplates.LocalHomeBookingOwnerNotificationTemplate(
						booking.LocalHome.Owner.FullName,
						booking.User.FullName,
						booking.LocalHome.Name,
						booking.StartDate,
						booking.EndDate,
						booking.TotalDays,
						booking.TotalAmount.ToString("F2"),
						booking.PaymentIntentId,
						booking.BookingDate
					);

					await _emailService.SendEmailAsync(
						booking.LocalHome.Owner.Email,
						"🏠 Your Local Home Has Been Booked!",
						ownerEmailContent
					);


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


		[HttpGet("user-bookings")]
		[Authorize]
		public async Task<IActionResult> GetUserBookings([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
		{
			try
			{
				var userId = _contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
				if (string.IsNullOrEmpty(userId))
				{
					return Unauthorized(new { IsSuccess = false, Message = "Invalid Token!" });
				}

				var query = _context.UserLocalHomesBookings
					.Where(b => b.UserId == Guid.Parse(userId))
					.OrderByDescending(b => b.Status)
					.Skip((page - 1) * pageSize)
					.Take(pageSize);

				var bookings = await query.Select(b => new
				{
					b.Id,
					b.LocalHomeId,
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
					Messege = "Validation failed.",
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

				var localHome = await _context.LocalHomes.FirstOrDefaultAsync(lh => lh.Id == request.ServiceId && !lh.IsDeleted && !lh.Owner.IsDeleted);
				if (localHome == null)
				{
					return NotFound(new DataSendingResponse
					{
						IsSuccess = false,
						Message = "Local home not found."
					});
				}

				var feedback = new Feedback
				{
					Id = Guid.NewGuid(),
					UserId = Guid.Parse(userId),
					ServiceType = Service.LocalHome.ToString(),
					ServiceId = request.ServiceId,
					FeedbackText = request.FeedbackText,
					Rating = request.Rating,
					Date = DateTime.Now
				};

				_context.FeedBacks.Add(feedback);
				await _context.SaveChangesAsync();

				// Update average rating
				var averageRating = await _context.FeedBacks
					.Where(f => f.ServiceId == request.ServiceId && f.ServiceType == Service.LocalHome.ToString())
					.AverageAsync(f => (decimal?)f.Rating) ?? 0;

				localHome.Rating = ((float)averageRating);
				_context.LocalHomes.Update(localHome);
				await _context.SaveChangesAsync();
				return Ok(new DataSendingResponse
				{
					IsSuccess = true,
					Data = feedback,
					Message = "Feedback added and rating updated successfully."
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

		[HttpGet("get-feedback/{localHomeId}")]
		public async Task<IActionResult> GetFeedback(Guid localHomeId)
		{
			try
			{
				var feedbacks = await _context.FeedBacks
				.Where(f => f.ServiceId == localHomeId && f.ServiceType == Service.LocalHome.ToString())
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
		[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		[Authorize(Roles = "LocalHomeOwner")]
		[HttpGet("/my-homes")]
		public async Task<IActionResult> GetLocalHomesOfLocalHomeOwner()
		{
			try
			{
				var userId = _contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
				if (string.IsNullOrEmpty(userId))
				{
					return Unauthorized(new DataSendingResponse { IsSuccess = false, Message = "Unauthorized action." });
				}
				var query = _context.LocalHomes.Where(h => h.OwnerId.ToString() == userId);

				var localHomes = await query.Select(lh => new
				{
					lh.Id,
					lh.Name,
					lh.Description,
					lh.AddressLine,
					lh.AvailableFrom,
					lh.AvailableTo,
					lh.Capacity,
					lh.PricePerNight,
					lh.Rating,
					ImageUrl = _context.ServiceImages
						.Where(img => img.ServiceId == lh.Id &&
									 img.ServiceType == Service.LocalHome.ToString() &&
									 img.IsPrimary)
						.Select(img => img.ImageUrl)
						.FirstOrDefault()
				}).ToListAsync();

				return Ok(new UserManagerResponse
				{
					Messege = "Local homes retrieved successfully",
					IsSuccess = true,
					Data = new
					{
						TotalRecords = localHomes.Count,
						LocalHomes = localHomes
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

	}

}
