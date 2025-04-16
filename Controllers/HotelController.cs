using IdealTrip.Models.Hotels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using IdealTrip.Models;
using IdealTrip.Models.LocalHome_Booking;
using IdealTrip.Models.Enums;
using IdealTrip.Services;
using IdealTrip.Helpers;
using IdealTrip.Models.Database_Tables;
using IdealTrip.Models.Package_Booking;
using Microsoft.AspNetCore.SignalR;

namespace IdealTrip.Controllers
{
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	[Authorize(Roles = "HotelOwner")]
	[Route("api/[controller]")]
	[ApiController]
	public class HotelController : ControllerBase
	{
		private readonly ApplicationDbContext _context;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly PaymentService _paymentService;
		private readonly IHubContext<NotificationHub> _hubContext;
		private readonly EmailService _emailService;
		public HotelController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, PaymentService paymentService, IHubContext<NotificationHub> hubContext, EmailService emailService)
		{
			_context = context;
			_httpContextAccessor = httpContextAccessor;
			_paymentService = paymentService;
			_hubContext = hubContext;
			_emailService = emailService;
		}

		// 1. Add Hotel
		[HttpPost]
		public async Task<IActionResult> AddHotel([FromForm] AddHotelModel model)
		{
			try
			{
				var ownerId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
				if (string.IsNullOrEmpty(ownerId))
				{
					return Unauthorized(new UserManagerResponse
					{
						IsSuccess = false,
						Messege = "User not authorized.",
						Errors = new List<string> { "User not authorized" }
					});
				}

				if (!ModelState.IsValid)
				{
					return BadRequest(new UserManagerResponse
					{
						IsSuccess = false,
						Messege = "Validation failed.",
						Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
					});
				}
				var hotel = new Hotel
				{
					HotelDescription = model.HotelDescription,
					Address = model.Address,
					HotelName = model.HotelName,
					CreatedAt = DateTime.Now,
					OwnerId = Guid.Parse(ownerId),
					IsAvailable = true,
					HotelId = Guid.NewGuid(),
					Rating = 0,
					UpdatedAt = DateTime.Now,
				};

				await _context.Hotels.AddAsync(hotel);
				await _context.SaveChangesAsync();
				var imageUrls = new List<string>();
				string homeFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/PropertyImages/Hotels", hotel.HotelId.ToString());

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

					var primaryImageUrl = $"/PropertyImages/Hotels/{hotel.HotelId}/{primaryFileName}";
					imageUrls.Add(primaryImageUrl);

					_context.ServiceImages.Add(new ServiceImage
					{
						ServiceId = hotel.HotelId,
						ServiceType = Service.Hotel.ToString(),
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

						var imageUrl = $"/PropertyImages/Hotels/{hotel.HotelId}/{fileName}";
						imageUrls.Add(imageUrl);

						_context.ServiceImages.Add(new ServiceImage
						{
							ServiceId = hotel.HotelId,
							ServiceType = Service.Hotel.ToString(),
							ImageUrl = imageUrl,
							IsPrimary = false // Additional images
						});
					}
					await _context.SaveChangesAsync();
				}

				return Ok(new UserManagerResponse { IsSuccess = true, Messege = "Hotel added successfully" });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new DataSendingResponse
				{
					IsSuccess = false,
					Message = "An error occurred while adding hotel.",
					Errors = new List<string> { ex.Message }
				});
			}
		}

		// 2. Update Hotel
		[HttpPost("{hotelId}")]
		public async Task<IActionResult> UpdateHotel(Guid hotelId, [FromForm] AddHotelModel model)
		{
			try
			{
				var ownerId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
				if (string.IsNullOrEmpty(ownerId))
				{
					return Unauthorized(new UserManagerResponse
					{
						IsSuccess = false,
						Messege = "User not authorized.",
						Errors = new List<string> { "User not authorized" }
					});
				}

				if (!ModelState.IsValid)
				{
					return BadRequest(new UserManagerResponse
					{
						IsSuccess = false,
						Messege = "Validation failed.",
						Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
					});
				}
				var hotel = await _context.Hotels.FirstOrDefaultAsync(h => h.HotelId == hotelId && h.OwnerId == Guid.Parse(ownerId) && !h.IsDeleted);
				if (hotel == null)
					return NotFound(new UserManagerResponse { IsSuccess = false, Messege = "Hotel not found!" });

				hotel.HotelName = model.HotelName;
				hotel.HotelDescription = model.HotelDescription;
				hotel.Address = model.Address;
				hotel.IsAvailable = model.IsAvailable;
				hotel.UpdatedAt = DateTime.Now;

				_context.Hotels.Update(hotel);
				await _context.SaveChangesAsync();
				var imageUrls = new List<string>();
				string homeFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/PropertyImages/Hotels", hotel.HotelId.ToString());

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

					var primaryImageUrl = $"/PropertyImages/Hotels/{hotel.HotelId}/{primaryFileName}";
					imageUrls.Add(primaryImageUrl);

					_context.ServiceImages.Add(new ServiceImage
					{
						ServiceId = hotel.HotelId,
						ServiceType = Service.Hotel.ToString(),
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

						var imageUrl = $"/PropertyImages/Hotels/{hotel.HotelId}/{fileName}";
						imageUrls.Add(imageUrl);

						_context.ServiceImages.Add(new ServiceImage
						{
							ServiceId = hotel.HotelId,
							ServiceType = Service.Hotel.ToString(),
							ImageUrl = imageUrl,
							IsPrimary = false // Additional images
						});
					}
					await _context.SaveChangesAsync();

				}

				return Ok(new UserManagerResponse { IsSuccess = true, Messege = "Hotel updated successfully" });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new DataSendingResponse
				{
					IsSuccess = false,
					Message = "An error occurred while updating hotel.",
					Errors = new List<string> { ex.Message }
				});
			}
		}

		// 3. Delete Hotel
		[HttpDelete("{hotelId}")]
		public async Task<IActionResult> DeleteHotel(Guid hotelId)
		{
			try
			{
				var ownerId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
				if (string.IsNullOrEmpty(ownerId))
				{
					return Unauthorized(new UserManagerResponse
					{
						IsSuccess = false,
						Messege = "User not authorized.",
						Errors = new List<string> { "User not authorized" }
					});
				}

				if (!ModelState.IsValid)
				{
					return BadRequest(new UserManagerResponse
					{
						IsSuccess = false,
						Messege = "Validation failed.",
						Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
					});
				}
				var hotel = await _context.Hotels.FirstOrDefaultAsync(h => h.HotelId == hotelId && h.OwnerId == Guid.Parse(ownerId) && !h.IsDeleted);
				if (hotel == null)
					return NotFound(new UserManagerResponse { IsSuccess = false, Messege = "Hotel not found!" });
				hotel.IsDeleted= true;
				_context.Hotels.Update(hotel);
				await _context.SaveChangesAsync();

				return Ok(new UserManagerResponse { IsSuccess = true, Messege = "Hotel deleted successfully" });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new UserManagerResponse { IsSuccess = false, Messege = "Internal Server Error" });
			}
		}

		// 4. Get My Hotels
		[HttpGet("my-hotels")]
		public async Task<IActionResult> GetMyHotels()
		{
			try
			{
				var ownerId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
				if (string.IsNullOrEmpty(ownerId))
				{
					return Unauthorized(new UserManagerResponse
					{
						IsSuccess = false,
						Messege = "User not authorized.",
						Errors = new List<string> { "User not authorized" }
					});
				}
				var hotels = await _context.Hotels
					.Where(h => h.OwnerId == Guid.Parse(ownerId) && !h.IsDeleted)
					.Select(h => new
					{
						h.HotelId,
						h.HotelName,
						h.HotelDescription,
						h.Address,
						h.IsAvailable,
						h.CreatedAt
					})
					.ToListAsync();

				return Ok(new UserManagerResponse { IsSuccess = true, Data = hotels });
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

		// 5. Add Hotel Room
		[HttpPost("add-room")]
		public async Task<IActionResult> AddRoom(Guid hotelId, [FromForm] AddHotelRoomModel model)
		{
			try
			{
				var ownerId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
				if (string.IsNullOrEmpty(ownerId))
				{
					return Unauthorized(new UserManagerResponse
					{
						IsSuccess = false,
						Messege = "User not authorized.",
						Errors = new List<string> { "User not authorized" }
					});
				}

				if (!ModelState.IsValid)
				{
					return BadRequest(new UserManagerResponse
					{
						IsSuccess = false,
						Messege = "Validation failed.",
						Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
					});
				}
				var hotel = await _context.Hotels.FirstOrDefaultAsync(h => h.HotelId == hotelId && h.OwnerId == Guid.Parse(ownerId) && !h.IsDeleted);
				if (hotel == null)
					return BadRequest(new UserManagerResponse { IsSuccess = false, Messege = "Invalid hotel or unauthorized!" });
				var hotelRoom = new HotelRoom
				{
					HotelId = hotelId,
					Capacity = model.Capacity,
					NumberOfBeds = model.NumberOfBeds,
					PricePerNight = model.PricePerNight,
					RoomType = model.RoomType,
					RoomId = Guid.NewGuid()
				};

				await _context.HotelRooms.AddAsync(hotelRoom);
				await _context.SaveChangesAsync();
				var imageUrls = new List<string>();
				string homeFolderPath = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot/PropertyImages/Hotels/{hotelId}", hotelRoom.RoomId.ToString());

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

					var primaryImageUrl = $"/PropertyImages/Hotels/{hotel.HotelId}/{hotelRoom.RoomId}/{primaryFileName}";
					imageUrls.Add(primaryImageUrl);

					_context.ServiceImages.Add(new ServiceImage
					{
						ServiceId = hotelRoom.RoomId,
						ServiceType = Service.HotelRoom.ToString(),
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

						var imageUrl = $"/PropertyImages/Hotels/{hotel.HotelId}/{hotelRoom.RoomId}/{fileName}";
						imageUrls.Add(imageUrl);

						_context.ServiceImages.Add(new ServiceImage
						{
							ServiceId = hotelRoom.RoomId,
							ServiceType = Service.HotelRoom.ToString(),
							ImageUrl = imageUrl,
							IsPrimary = false // Additional images
						});
					}
					await _context.SaveChangesAsync();
				}

				return Ok(new UserManagerResponse { IsSuccess = true, Messege = "Room added successfully" });
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
		[HttpGet("{hotelId}/rooms")]
		public async Task<IActionResult> GetHotelRooms(Guid hotelId)
		{
			try
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

				// Check if the hotel exists and belongs to the logged-in owner
				var hotelExists = await _context.Hotels.AnyAsync(h => h.HotelId == hotelId && !h.IsDeleted);
				if (!hotelExists)
					return NotFound(new UserManagerResponse { IsSuccess = false, Messege = "Hotel not found or unauthorized!" });

				// Get the rooms for that hotel
				var rooms = await _context.HotelRooms
					.Where(r => r.HotelId == hotelId)
					.Select(r => new
					{
						r.RoomId,
						r.RoomType,
						r.Capacity,
						r.NumberOfBeds,
						r.PricePerNight,
						r.IsAvailable
					})
					.ToListAsync();

				return Ok(new { IsSuccess = true, Rooms = rooms });
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


		// 7. Update Room
		[HttpPut("room/{roomId}")]
		public async Task<IActionResult> UpdateRoom(Guid roomId, [FromBody] AddHotelRoomModel updatedRoom)
		{
			try
			{
				var ownerId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
				if (string.IsNullOrEmpty(ownerId))
				{
					return Unauthorized(new UserManagerResponse
					{
						IsSuccess = false,
						Messege = "User not authorized.",
						Errors = new List<string> { "User not authorized" }
					});
				}

				if (!ModelState.IsValid)
				{
					return BadRequest(new UserManagerResponse
					{
						IsSuccess = false,
						Messege = "Validation failed.",
						Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
					});
				}
				var room = await _context.HotelRooms.Include(r => r.Hotel).FirstOrDefaultAsync(r => r.RoomId == roomId && r.Hotel.OwnerId == Guid.Parse(ownerId) && !r.Hotel.IsDeleted && !r.IsDeleted);
				string hotelId = _context.HotelRooms.FirstOrDefault(r => r.RoomId == roomId && r.Hotel.OwnerId == Guid.Parse(ownerId)).HotelId.ToString();
				if (room == null)
					return NotFound(new UserManagerResponse { IsSuccess = false, Messege = "Room not found" });

				room.PricePerNight = updatedRoom.PricePerNight;
				room.Capacity = updatedRoom.Capacity;
				room.NumberOfBeds = updatedRoom.NumberOfBeds;
				room.RoomType = updatedRoom.RoomType;

				_context.HotelRooms.Update(room);
				await _context.SaveChangesAsync();
				var imageUrls = new List<string>();
				string homeFolderPath = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot/PropertyImages/Hotels/{hotelId}", roomId.ToString());

				if (!Directory.Exists(homeFolderPath))
				{
					Directory.CreateDirectory(homeFolderPath);
				}

				// Handle Primary Image
				if (updatedRoom.PrimaryImage != null)
				{
					var primaryFileName = $"primary_{Guid.NewGuid()}_{Path.GetFileName(updatedRoom.PrimaryImage.FileName)}";
					var primaryFilePath = Path.Combine(homeFolderPath, primaryFileName);

					using (var stream = new FileStream(primaryFilePath, FileMode.Create))
					{
						await updatedRoom.PrimaryImage.CopyToAsync(stream);
					}

					var primaryImageUrl = $"/PropertyImages/Hotels/{hotelId}/{roomId}/{primaryFileName}";
					imageUrls.Add(primaryImageUrl);

					_context.ServiceImages.Add(new ServiceImage
					{
						ServiceId = roomId,
						ServiceType = Service.HotelRoom.ToString(),
						ImageUrl = primaryImageUrl,
						IsPrimary = true // Mark this as the primary image
					});
				}

				// Handle Additional Images
				if (updatedRoom.Images != null && updatedRoom.Images.Count > 0)
				{
					foreach (var image in updatedRoom.Images)
					{
						var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(image.FileName)}";
						var filePath = Path.Combine(homeFolderPath, fileName);

						using (var stream = new FileStream(filePath, FileMode.Create))
						{
							await image.CopyToAsync(stream);
						}

						var imageUrl = $"/PropertyImages/Hotels/{hotelId}/{roomId}/{fileName}";
						imageUrls.Add(imageUrl);

						_context.ServiceImages.Add(new ServiceImage
						{
							ServiceId = roomId,
							ServiceType = Service.HotelRoom.ToString(),
							ImageUrl = imageUrl,
							IsPrimary = false // Additional images
						});
					}
					await _context.SaveChangesAsync();
				}

				return Ok(new UserManagerResponse { IsSuccess = true, Messege = "Room updated successfully" });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new DataSendingResponse
				{
					IsSuccess = false,
					Message = "Internal Server Error	",
					Errors = new List<string> { ex.Message }
				});
			}
		}

		// 8. Delete Room
		[HttpDelete("room/{roomId}")]
		public async Task<IActionResult> DeleteRoom(Guid roomId)
		{
			try
			{
				var ownerId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
				if (string.IsNullOrEmpty(ownerId))
				{
					return Unauthorized(new UserManagerResponse
					{
						IsSuccess = false,
						Messege = "User not authorized.",
						Errors = new List<string> { "User not authorized" }
					});
				}
				var room = await _context.HotelRooms.Include(r => r.Hotel).FirstOrDefaultAsync(r => r.RoomId == roomId && r.Hotel.OwnerId == Guid.Parse(ownerId) && !r.IsDeleted && !r.Hotel.IsDeleted);
				if (room == null)
					return NotFound(new UserManagerResponse { IsSuccess = false, Messege = "Room not found or unauthorized!" });

				room.IsDeleted = true;
				_context.HotelRooms.Update(room);
				await _context.SaveChangesAsync();

				return Ok(new UserManagerResponse { IsSuccess = true, Messege = "Room deleted successfully" });
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


		[HttpGet]
		[AllowAnonymous]
		public async Task<IActionResult> GetHotelsWithFilters(
	string? name,
	string? address,
	RoomType? roomType,
	decimal? minPrice,
	decimal? maxPrice,
	int? minCapacity,
	int? minBeds)
		{
			try
			{
				var query = _context.Hotels
					.Where(h => h.IsAvailable && !h.IsDeleted && !h.Owner.IsDeleted)
					.Include(h => h.Owner)
					.AsQueryable();

				if (!string.IsNullOrEmpty(name))
					query = query.Where(h => h.HotelName.Contains(name));
				if (!string.IsNullOrEmpty(address))
					query = query.Where(h => h.Address.Contains(address));

				var hotels = await query.ToListAsync();

				var hotelDtos = new List<object>();

				foreach (var hotel in hotels)
				{
					var roomsQuery = _context.HotelRooms
						.Where(r => r.HotelId == hotel.HotelId && r.IsAvailable);

					if (roomType.HasValue)
						roomsQuery = roomsQuery.Where(r => r.RoomType == roomType.Value);
					if (minPrice.HasValue)
						roomsQuery = roomsQuery.Where(r => r.PricePerNight >= minPrice.Value);
					if (maxPrice.HasValue)
						roomsQuery = roomsQuery.Where(r => r.PricePerNight <= maxPrice.Value);
					if (minCapacity.HasValue)
						roomsQuery = roomsQuery.Where(r => r.Capacity >= minCapacity.Value);
					if (minBeds.HasValue)
						roomsQuery = roomsQuery.Where(r => r.NumberOfBeds >= minBeds.Value);

					var rooms = await roomsQuery.ToListAsync();

					if (rooms.Any())
					{
						hotelDtos.Add(new
						{
							hotel.HotelId,
							hotel.HotelName,
							hotel.HotelDescription,
							hotel.Address,
							hotel.IsAvailable,
							hotel.Rating,
							Rooms = rooms.Select(r => new
							{
								r.RoomId,
								RoomType = ((RoomType)r.RoomType).ToString(),
								r.PricePerNight,
								r.Capacity,
								r.NumberOfBeds
							})
						});
					}
				}

				return Ok(new DataSendingResponse
				{
					Data = hotelDtos,
					IsSuccess = true,
					Message = "Hotels with available rooms fetched successfully"
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
		[Authorize]
		[HttpGet("{id}")]
		public async Task<IActionResult> GetHotelById(Guid id)
		{
			try
			{
				var hotel = await _context.Hotels
					.Include(h => h.Owner)
					.FirstOrDefaultAsync(h => h.HotelId == id && h.IsAvailable && !h.Owner.IsDeleted);

				if (hotel == null || hotel.IsDeleted)
					return NotFound(new DataSendingResponse { IsSuccess = true, Message = "Hotel not found" });

				var rooms = await _context.HotelRooms
					.Where(r => r.HotelId == id && r.IsAvailable)
					.ToListAsync();

				return Ok(new DataSendingResponse
				{
					IsSuccess = true,
					Data = new
					{
						hotel.HotelId,
						hotel.HotelName,
						hotel.HotelDescription,
						hotel.Address,
						hotel.IsAvailable,
						hotel.Rating,
						Rooms = rooms.Select(r => new
						{
							r.RoomId,
							r.RoomType,
							r.PricePerNight,
							r.Capacity,
							r.NumberOfBeds
						})
					},
					Message = "Hotel details fetched"
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
		[HttpGet("/rooms")]
		[AllowAnonymous]
		public async Task<IActionResult> GetRoomsWithFilters(
	Guid? hotelId,
	RoomType? roomType,
	decimal? minPrice,
	decimal? maxPrice,
	int? minCapacity,
	int? minBeds)
		{
			try
			{
				var query = _context.HotelRooms
					.Include(r => r.Hotel)
					.Where(r => r.IsAvailable && r.Hotel.IsAvailable && !r.IsDeleted && !r.Hotel.IsDeleted && !r.Hotel.Owner.IsDeleted)
					.AsQueryable();

				if (hotelId.HasValue)
					query = query.Where(r => r.HotelId == hotelId.Value);

				if (roomType.HasValue)
					query = query.Where(r => r.RoomType == roomType.Value);

				if (minPrice.HasValue)
					query = query.Where(r => r.PricePerNight >= minPrice.Value);

				if (maxPrice.HasValue)
					query = query.Where(r => r.PricePerNight <= maxPrice.Value);

				if (minCapacity.HasValue)
					query = query.Where(r => r.Capacity >= minCapacity.Value);

				if (minBeds.HasValue)
					query = query.Where(r => r.NumberOfBeds >= minBeds.Value);

				var rooms = await query.ToListAsync();

				var roomDtos = rooms.Select(r => new
				{
					r.RoomId,
					r.RoomType,
					r.PricePerNight,
					r.Capacity,
					r.NumberOfBeds,
					Hotel = new
					{
						r.Hotel.HotelId,
						r.Hotel.HotelName,
						r.Hotel.Address,
						r.Hotel.Rating
					}
				});

				return Ok(new DataSendingResponse
				{
					IsSuccess = true,
					Message = "Rooms fetched successfully",
					Data = roomDtos
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
		[HttpGet("rooms/{id}")]
		public async Task<IActionResult> GetRoomById(Guid id)
		{
			try
			{
				var room = await _context.HotelRooms
					.Include(r => r.Hotel)
					.FirstOrDefaultAsync(r => r.RoomId == id && r.IsAvailable && r.Hotel.IsAvailable && !r.IsDeleted && !r.Hotel.IsDeleted && !r.Hotel.Owner.IsDeleted);

				if (room == null )
				{
					return NotFound(new DataSendingResponse
					{
						IsSuccess = false,
						Message = "Room not found or not available"
					});
				}

				var roomDto = new
				{
					room.RoomId,
					room.RoomType,
					room.PricePerNight,
					room.Capacity,
					room.NumberOfBeds,
					Hotel = new
					{
						room.Hotel.HotelId,
						room.Hotel.HotelName,
						room.Hotel.HotelDescription,
						room.Hotel.Address,
						room.Hotel.Rating
					}
				};

				return Ok(new DataSendingResponse
				{
					IsSuccess = true,
					Message = "Room fetched successfully",
					Data = roomDto
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
		public async Task<IActionResult> InitiateHotelBooking([FromBody] AddUserHotelBookingModel booking)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(new UserManagerResponse
				{
					IsSuccess = false,
					Messege = "Invalid booking Data",
					Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
				});
			}

			try
			{
				var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
				if (string.IsNullOrEmpty(userId))
					return Unauthorized(new DataSendingResponse { IsSuccess = false, Message = "Unauthorized access." });

				if (booking.StartDate >= booking.EndDate)
					return BadRequest(new DataSendingResponse { IsSuccess = false, Message = "Start date must be before End Date." });

				var hotelRoom = await _context.HotelRooms.FindAsync(booking.HotelRoomId);
				if (hotelRoom == null || hotelRoom.IsDeleted || hotelRoom.Hotel.IsDeleted || hotelRoom.Hotel.Owner.IsDeleted)
					return NotFound(new DataSendingResponse { IsSuccess = false, Message = "Room not available." });

				// Booking conflict check
				var existingBookings = await _context.UserHotelRoomBookings
					.Where(b => b.RoomId.ToString() == booking.HotelRoomId && ( b.Status == BookingStatus.Paid.ToString()))
					.ToListAsync();

				bool hasConflict = existingBookings.Any(existing =>
					(booking.StartDate >= existing.CheckInDate && booking.StartDate <= existing.CheckOutDate) ||
					(booking.EndDate >= existing.CheckInDate && booking.EndDate <= existing.CheckOutDate) ||
					(booking.StartDate <= existing.CheckInDate && booking.EndDate >= existing.CheckOutDate));

				if (hasConflict)
					return Conflict(new DataSendingResponse { IsSuccess = false, Message = "Room already booked for selected dates." });

				decimal totalCost = booking.TotalDays * hotelRoom.PricePerNight;

				var newBooking = new UserHotelRoomBooking
				{
					BookingId = Guid.NewGuid(),
					UserId = Guid.Parse(userId),
					RoomId = Guid.Parse(booking.HotelRoomId),
					TotalAmount = totalCost,
					Status = BookingStatus.Pending.ToString(),
					CheckInDate = booking.StartDate,
					CheckOutDate = booking.EndDate,
					TotalDays = booking.TotalDays,
					BookingTime = DateTime.Now
				};

				_context.UserHotelRoomBookings.Add(newBooking);
				await _context.SaveChangesAsync();

				var paymentResult = await _paymentService.CreatePaymentIntent(newBooking.BookingId, "HotelRoom", newBooking.TotalAmount);
				var clientSecret = paymentResult?.GetType().GetProperty("clientSecret")?.GetValue(paymentResult)?.ToString();

				if (!string.IsNullOrEmpty(clientSecret))
				{
					return Ok(new DataSendingResponse
					{
						IsSuccess = true,
						Message = "Booking initiated successfully.",
						Data = new { BookingId = newBooking.BookingId, ClientSecret = clientSecret }
					});
				}
				else
				{
					return BadRequest(new DataSendingResponse { IsSuccess = false, Message = "Failed to create payment intent." });
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
		public async Task<IActionResult> HotelBookingPaymentSuccess([FromBody] PaymentSuccessDto paymentData)
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
				var bookingId = Guid.Parse(paymentData.BookingId);
				var booking = await _context.UserHotelRoomBookings
					.Include(b => b.Tourist)
					.Include(b => b.HotelRoom).ThenInclude(r => r.Hotel).ThenInclude(h => h.Owner)
					.FirstOrDefaultAsync(b => b.BookingId == bookingId);

				if (booking == null)
					return NotFound(new DataSendingResponse { IsSuccess = false, Message = "Booking not found." });

				using var transaction = await _context.Database.BeginTransactionAsync();
				booking.Status = BookingStatus.Paid.ToString();
				booking.PaymentIntentId = paymentData.PaymentIntentId;

				_context.UserHotelRoomBookings.Update(booking);
				await _context.SaveChangesAsync();

				// Notify hotel owner
				string ownerMsg = $"🏨 Your hotel room in '{booking.HotelRoom.Hotel.HotelName}' was booked by {booking.Tourist.FullName}. 💳 Payment: {paymentData.PaymentIntentId}";
				_context.Notifications.Add(new Notifications
				{
					UserId = booking.HotelRoom.Hotel.Owner.Id,
					Messege = ownerMsg
				});
				await _hubContext.Clients.User(booking.HotelRoom.Hotel.Owner.Id.ToString())
								.SendAsync("ReceiveNotification", ownerMsg);

				// Notify user
				string userMsg = $"🎉 Your hotel booking for '{booking.HotelRoom.Hotel.HotelName}' is confirmed!";
				_context.Notifications.Add(new Notifications
				{
					UserId = booking.Tourist.Id,
					Messege = userMsg
				});
				await _hubContext.Clients.User(booking.Tourist.Id.ToString())
								.SendAsync("ReceiveNotification", userMsg);

				await _context.SaveChangesAsync();

				// Send email
				var emailContent = EmailTemplates.HotelBookingSuccessTemplate(
		booking.Tourist.FullName,
	booking.Tourist.Email,
	booking.HotelRoom.Hotel.HotelName,
	booking.HotelRoom.RoomType.ToString(),
	booking.TotalAmount,
	booking.PaymentIntentId,
	booking.CheckInDate.ToString("dd MMM yyyy") ?? "N/A",
	booking.CheckOutDate.ToString("dd MMM yyyy") ?? "N/A",
	booking.Status,
	booking.BookingTime
);

				await _emailService.SendEmailAsync(booking.Tourist.Email, "Hotel Booking Confirmation", emailContent);
				var ownerEmailContent = EmailTemplates.HotelOwnerBookingNotificationTemplate(
	booking.HotelRoom.Hotel.Owner.FullName,
	booking.Tourist.FullName,
	booking.Tourist.Email,
	booking.HotelRoom.Hotel.HotelName,
	booking.HotelRoom.RoomType.ToString(),
	booking.CheckInDate,
	booking.CheckOutDate,
	paymentData.PaymentIntentId,
	booking.TotalAmount,
	booking.BookingTime
);

				await _emailService.SendEmailAsync(booking.HotelRoom.Hotel.Owner.Email, "🏨 New Hotel Booking Received", ownerEmailContent);


				await transaction.CommitAsync();

				return Ok(new DataSendingResponse { IsSuccess = true, Message = "Payment recorded and notifications sent." });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new DataSendingResponse
				{
					IsSuccess = false,
					Message = $"Server error: {ex.Message}"
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
				var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
				if (string.IsNullOrEmpty(userId))
					return Unauthorized(new DataSendingResponse { IsSuccess = false, Message = "Unauthorized action." });

				var hotel = await _context.Hotels.FirstOrDefaultAsync(h => h.HotelId == request.ServiceId && !h.IsDeleted && !h.Owner.IsDeleted);
				if (hotel == null)
					return NotFound(new DataSendingResponse { IsSuccess = false, Message = "Hotel not found." });

				var feedback = new Feedback
				{
					Id = Guid.NewGuid(),
					UserId = Guid.Parse(userId),
					ServiceType = Service.Hotel.ToString(),
					ServiceId = request.ServiceId,
					FeedbackText = request.FeedbackText,
					Rating = request.Rating,
					Date = DateTime.Now
				};

				_context.FeedBacks.Add(feedback);
				await _context.SaveChangesAsync();

				// Update hotel rating
				var avgRating = await _context.FeedBacks
					.Where(f => f.ServiceId == request.ServiceId && f.ServiceType == Service.Hotel.ToString())
					.AverageAsync(f => (decimal?)f.Rating) ?? 0;

				hotel.Rating = (float)avgRating;
				_context.Hotels.Update(hotel);
				await _context.SaveChangesAsync();

				return Ok(new DataSendingResponse { IsSuccess = true, Data = feedback, Message = "Feedback added successfully." });
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
		[HttpGet("hotel/get-feedback/{hotelId}")]
		public async Task<IActionResult> GetHotelFeedbacks(Guid hotelId)
		{
			try
			{
				var feedbacks = await _context.FeedBacks
					.Where(f => f.ServiceId == hotelId && f.ServiceType == Service.Hotel.ToString())
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
					Message = "Internal Server Error",
					Errors = new List<string> { ex.Message }
				});
			}
		}



	}
}
