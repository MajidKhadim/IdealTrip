using IdealTrip.Models.LocalHome_Booking;
using IdealTrip.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IdealTrip.Controllers
{
	public class HotelRoomsController : ControllerBase
	{
		private readonly IHttpContextAccessor _contextAccessor;
		private readonly ApplicationDbContext _context;
        public HotelRoomsController(IHttpContextAccessor contextAccessor,ApplicationDbContext context)
        {
            _contextAccessor = contextAccessor;
			_context = context;
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
	}
}
