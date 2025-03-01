using IdealTrip.Helpers;
using IdealTrip.Models;
using IdealTrip.Models.Enums;
using IdealTrip.Models.LocalHome_Booking;
using IdealTrip.Models.Package_Booking;
using IdealTrip.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

		public LocalHomeController(ApplicationDbContext context, IHttpContextAccessor contextAccessor, PaymentService paymentService)
		{
			_context = context;
			_contextAccessor = contextAccessor;
			_paymentService = paymentService;
		}

		[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		[Authorize(Roles = "LocalHomeOwner")]
		[HttpPost("add-localhome")]
		public async Task<IActionResult> AddLocalHome([FromForm] AddLocalHomeModel model)
		{
			var userId = _contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized(new DataSendingResponse { IsSuccess = false, Message = "Unauthorized action." });
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
				IsAvailable = true
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
		[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		[Authorize(Roles = "LocalHomeOwner")]
		[HttpPut("update-localhome/{id}")]
		public async Task<IActionResult> UpdateLocalHome(Guid id, [FromForm] AddLocalHomeModel updatedHome)
		{
			var userId = _contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
			var home = await _context.LocalHomes.FirstOrDefaultAsync(h => h.Id == id);
			if (home == null)
			{
				return NotFound(new DataSendingResponse { IsSuccess = false, Message = "Local home not found." });
			}
			if(userId != home.OwnerId.ToString())
			{
				return Forbid();
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

		[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		[Authorize(Roles = "LocalHomeOwner")]
		[HttpDelete("delete-localhome/{id}")]
		public async Task<IActionResult> DeleteLocalHome(Guid id)
		{
			var userId = _contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
			var home = await _context.LocalHomes.FirstOrDefaultAsync(h => h.Id == id);
			if (home == null)
			{
				return NotFound(new DataSendingResponse { IsSuccess = false, Message = "Local home not found." });
			}
			if(userId != home.OwnerId.ToString())
			{
				return Forbid();
			}

			// ✅ Fetch images from the ServiceImages table
			var serviceImages = await _context.ServiceImages
				.Where(img => img.ServiceId == home.Id && img.ServiceType == Service.LocalHome.ToString())
				.ToListAsync();

			// ✅ Delete images from the folder
			string homeFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/PropertyImages/LocalHomes", home.Id.ToString());
			if (Directory.Exists(homeFolderPath))
			{
				Directory.Delete(homeFolderPath, true);
			}

			// ✅ Remove images from the database
			_context.ServiceImages.RemoveRange(serviceImages);

			// ✅ Remove the LocalHome record
			_context.LocalHomes.Remove(home);
			await _context.SaveChangesAsync();

			return Ok(new DataSendingResponse
			{
				IsSuccess = true,
				Message = "Local home and associated images deleted successfully."
			});
		}

		[HttpGet("GetLocalHomes")]
		public async Task<IActionResult> GetLocalHomes([FromQuery] string? location = null, [FromQuery] decimal? minPrice = null, [FromQuery] decimal? maxPrice = null,
[FromQuery] int? minCapacity = null)
		{
			var query = _context.LocalHomes.Where(h => h.IsAvailable);

			if (!string.IsNullOrEmpty(location))
				query = query.Where(h => h.AddressLine.Contains(location));

			if (minPrice.HasValue)
				query = query.Where(h => h.PricePerNight >= minPrice.Value);

			if (maxPrice.HasValue)
				query = query.Where(h => h.PricePerNight <= maxPrice.Value);

			if (minCapacity.HasValue)
				query = query.Where(h => h.Capacity >= minCapacity.Value);

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
		.Where(img => img.ServiceId == lh.Id && img.ServiceType == Service.LocalHome.ToString() && img.IsPrimary)
		.Select(img => img.ImageUrl)
		.FirstOrDefault() // Returns a single image URL or null if not found
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

		[HttpGet("GetLocalHomeById/{id}")]
		public async Task<IActionResult> GetLocalHomeById(Guid id)
		{
			var localHome = await _context.LocalHomes
				.Where(h => h.Id == id && h.IsAvailable)
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
					MainImage = _context.ServiceImages
						.Where(img => img.ServiceId == lh.Id && img.ServiceType == Service.LocalHome.ToString() && img.IsPrimary)
						.Select(img => img.ImageUrl)
						.FirstOrDefault(),
					Images = _context.ServiceImages
						.Where(img => img.ServiceId == lh.Id && img.ServiceType == Service.LocalHome.ToString()&& img.IsPrimary!=true)
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


		[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		[Authorize(Roles = "Tourist")]
		[HttpPost("booking/initiate")]
		public async Task<IActionResult> InitiateBooking([FromBody] LocalHomeBookingModel booking)
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

			var localHome = await _context.LocalHomes.FindAsync(booking.LocalHomeId);
			if (localHome == null || !localHome.IsAvailable)
			{
				return BadRequest(new { IsSuccess = false, Message = "Local home not available!" });
			}

			// Calculate total cost
			decimal totalCost = booking.TotalDays * localHome.PricePerNight;

			var pendingBooking = new UserLocalHomeBooking
			{
				UserId = Guid.Parse(userId),
				LocalHomeId = booking.LocalHomeId,
				TotalAmount = totalCost,
				Status = "Pending",
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
		[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		[Authorize(Roles = "Tourist")]

		[HttpPost("booking/payment-success")]
		public async Task<IActionResult> PaymentSuccess([FromBody] PaymentSuccessDto paymentData)
		{
			if (paymentData == null || string.IsNullOrEmpty(paymentData.BookingId) || string.IsNullOrEmpty(paymentData.PaymentIntentId))
			{
				return BadRequest(new { IsSuccess = false, Message = "Invalid request data." });
			}

			var bookingId = Guid.Parse(paymentData.BookingId);
			var booking = await _context.UserLocalHomesBookings.FindAsync(bookingId);

			if (booking == null)
			{
				return NotFound(new { IsSuccess = false, Message = "Booking not found." });
			}

			using var transaction = await _context.Database.BeginTransactionAsync();
			try
			{
				booking.Status = "Paid";
				booking.PaymentIntentId = paymentData.PaymentIntentId;
				_context.UserLocalHomesBookings.Update(booking);
				await _context.SaveChangesAsync();

				string content = EmailTemplates.PaymentSuccessTemplate(booking.User.FullName, booking.TotalAmount.ToString(), booking.BookingDate.ToString(), booking.LocalHome.Name, booking.LocalHome.Description, booking.Status, booking.PaymentIntentId);
				await _emailService.SendEmailAsync(booking.User.Email, "Booking Successful", content);

				await transaction.CommitAsync();
				return Ok(new { IsSuccess = true, Message = "Payment updated successfully. Confirmation email sent." });
			}
			catch
			{
				await transaction.RollbackAsync();
				return BadRequest(new { IsSuccess = false, Message = "Payment processing failed." });
			}

		}

		[HttpGet("user-bookings")]
		public async Task<IActionResult> GetUserBookings([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
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
		[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		[Authorize(Roles = "Tourist")]
		[HttpPost("add-feedback")]
		public async Task<IActionResult> AddFeedback([FromBody] FeedbackRequest request)
		{
			var userId = _contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized(new DataSendingResponse { IsSuccess = false, Message = "Unauthorized action." });
			}

			var localHome = await _context.LocalHomes.FirstOrDefaultAsync(lh => lh.Id == request.ServiceId);
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
		[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		[Authorize(Roles = "Tourist")]

		[HttpGet("get-feedback/{localHomeId}")]
		public async Task<IActionResult> GetFeedback(Guid localHomeId)
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

	}

}
