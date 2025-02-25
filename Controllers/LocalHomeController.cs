using IdealTrip.Models;
using IdealTrip.Models.Enums;
using IdealTrip.Models.LocalHome_Booking;
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
	[Authorize(Roles = "LocalHomeOwner")]
	public class LocalHomeController : ControllerBase
	{
		private readonly ApplicationDbContext _context;
		private readonly IHttpContextAccessor _contextAccessor;

		public LocalHomeController(ApplicationDbContext context, IHttpContextAccessor contextAccessor)
		{
			_context = context;
			_contextAccessor = contextAccessor;
		}

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

			// Save Images in subfolder with LocalHomeId
			var imageUrls = new List<string>();
			if (model.Images != null && model.Images.Count > 0)
			{
				string homeFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/PropertyImages/LocalHomes", localHomeId.ToString());
				if (!Directory.Exists(homeFolderPath))
				{
					Directory.CreateDirectory(homeFolderPath);
				}

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
						Id = Guid.NewGuid(),
						ServiceId = localHome.Id,
						ServiceType = Service.LocalHome.ToString(),
						ImageUrl = imageUrl,
						IsPrimary = false
					});
				}
				await _context.SaveChangesAsync();
			}

			return Ok(new DataSendingResponse
			{
				IsSuccess = true,
				Data = new { localHome, Images = imageUrls },
				Message = "Local home added successfully with images."
			});
		}

		// ✅ Get Local Homes with Images
		//[HttpGet("get-localhomes")]
		//public async Task<IActionResult> GetLocalHomes()
		//{
		//	var ownerId = _contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
		//	if (string.IsNullOrEmpty(ownerId))
		//	{
		//		return Unauthorized(new DataSendingResponse { IsSuccess = false, Message = "Unauthorized action." });
		//	}

		//	var homes = await _context.LocalHomes
		//		.Where(h => h.OwnerId == Guid.Parse(ownerId))
		//		.Include(h => h.Images) // Assuming navigation property
		//		.ToListAsync();

		//	var result = homes.Select(home => new
		//	{
		//		home.Id,
		//		home.Name,
		//		home.Description,
		//		home.AddressLine,
		//		home.PricePerNight,
		//		home.Capacity,
		//		home.AvailableFrom,
		//		home.AvailableTo,
		//		home.IsAvailable,
		//		Images = home.Images.Select(img => img.ImageUrl)
		//	});

		//	return Ok(new DataSendingResponse
		//	{
		//		IsSuccess = true,
		//		Data = result,
		//		Message = "Local homes retrieved successfully."
		//	});
		//}

		//// ✅ Update Local Home with Optional Image Update
		//[HttpPut("update-localhome/{id}")]
		//public async Task<IActionResult> UpdateLocalHome(Guid id, [FromForm] AddLocalHomeModel updatedHome)
		//{
		//	var home = await _context.LocalHomes.Include(h => h.Images).FirstOrDefaultAsync(h => h.Id == id);
		//	if (home == null)
		//	{
		//		return NotFound(new DataSendingResponse { IsSuccess = false, Message = "Local home not found." });
		//	}

		//	// Update fields
		//	home.Name = updatedHome.Name;
		//	home.Description = updatedHome.Description;
		//	home.AddressLine = updatedHome.AddressLine;
		//	home.PricePerNight = updatedHome.PricePerNight;
		//	home.Capacity = updatedHome.Capacity;
		//	home.AvailableFrom = updatedHome.AvailableFrom;
		//	home.AvailableTo = updatedHome.AvailableTo;
		//	home.UpdatedAt = DateTime.UtcNow;

		//	// Handle Images Update
		//	if (updatedHome.Images != null && updatedHome.Images.Count > 0)
		//	{
		//		string homeFolderPath = Path.Combine(_localHomeBasePath, home.Id.ToString());

		//		// Delete old images from folder and DB
		//		if (Directory.Exists(homeFolderPath))
		//		{
		//			Directory.Delete(homeFolderPath, true);
		//		}
		//		Directory.CreateDirectory(homeFolderPath);
		//		_context.ServiceImages.RemoveRange(home.Images);

		//		foreach (var image in updatedHome.Images)
		//		{
		//			var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(image.FileName)}";
		//			var filePath = Path.Combine(homeFolderPath, fileName);

		//			using (var stream = new FileStream(filePath, FileMode.Create))
		//			{
		//				await image.CopyToAsync(stream);
		//			}

		//			var imageUrl = $"/PropertyImages/LocalHomes/{home.Id}/{fileName}";
		//			_context.ServiceImages.Add(new ServiceImage
		//			{
		//				Id = Guid.NewGuid(),
		//				ServiceId = home.Id,
		//				ServiceType = Service.LocalHome.ToString(),
		//				ImageUrl = imageUrl,
		//				IsPrimary = false
		//			});
		//		}
		//	}

		//	await _context.SaveChangesAsync();

		//	return Ok(new DataSendingResponse
		//	{
		//		IsSuccess = true,
		//		Data = home,
		//		Message = "Local home updated successfully."
		//	});
		//}

		//// ✅ Delete Local Home with Images
		//[HttpDelete("delete-localhome/{id}")]
		//public async Task<IActionResult> DeleteLocalHome(Guid id)
		//{
		//	var home = await _context.LocalHomes.Include(h => h.Images).FirstOrDefaultAsync(h => h.Id == id);
		//	if (home == null)
		//	{
		//		return NotFound(new DataSendingResponse { IsSuccess = false, Message = "Local home not found." });
		//	}

		//	// Delete folder and images
		//	string homeFolderPath = Path.Combine(_localHomeBasePath, home.Id.ToString());
		//	if (Directory.Exists(homeFolderPath))
		//	{
		//		Directory.Delete(homeFolderPath, true);
		//	}

		//	_context.ServiceImages.RemoveRange(home.Images);
		//	_context.LocalHomes.Remove(home);
		//	await _context.SaveChangesAsync();

		//	return Ok(new DataSendingResponse
		//	{
		//		IsSuccess = true,
		//		Message = "Local home and associated images deleted successfully."
		//	});
		//}
	}

}
