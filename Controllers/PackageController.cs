using IdealTrip.Models;
using IdealTrip.Models.Package_Booking;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe.Checkout;

namespace IdealTrip.Controllers
{
    [Route("api/[controller]")]
    public class PackageController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PackageController> _logger;
		private readonly IConfiguration _configuration;
        public PackageController(ApplicationDbContext context, ILogger<PackageController> logger,IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
			_configuration = configuration;
        }

        [HttpGet]
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
		//[HttpPost("/booking")]
		//public async Task<IActionResult> CreateBooking([FromBody] UsersPackageBooking booking)
		//{
		//	try
		//	{
		//		// Check if the package exists
		//		var package = await _context.Packages.FindAsync(booking.PackageId);
		//		if (package == null)
		//		{
		//			return NotFound(new UserManagerResponse { IsSuccess = false, Messege = "Package not found!" });
		//		}

		//		// Check if there are enough available spots
		//		if (package.AvailableSpots < booking.NumberOfTravelers)
		//		{
		//			return BadRequest(new UserManagerResponse { IsSuccess = false, Messege = "Not enough available spots!" });
		//		}

		//		// Calculate the total bill
		//		booking.TotalBill = package.Price * booking.NumberOfTravelers;

		//		// Save the booking
		//		_context.UsersPackages.Add(booking);
		//		package.AvailableSpots -= booking.NumberOfTravelers; // Update available spots
		//		await _context.SaveChangesAsync();

		//		return Ok(new UserManagerResponse{ IsSuccess = true, Messege = "Booking successful!" });
		//	}
		//	catch (Exception ex)
		//	{
		//		_logger.LogError($"Error while creating booking: {ex.Message}");
		//		return StatusCode(500, new UserManagerResponse{ IsSuccess = false, Messege = "Something went wrong!" });
		//	}
		//}
			[HttpPost("booking/initiate")]
			public async Task<IActionResult> InitiateBooking([FromBody] BookingModel booking)
			{
				var package = await _context.Packages.FindAsync(booking.PackageId);
				
			if (package == null || package.AvailableSpots < booking.NumberOfTravelers)
				{
					return BadRequest(new { IsSuccess = false, Message = "Invalid package or insufficient spots!" });
				}

				// Save booking as pending
				var pendingBooking = new UsersPackageBooking
				{
					UserId = booking.Id,
					FullName = booking.FullName,
					Email = booking.Email,
					PhoneNumber = booking.PhoneNumber,
					NumberOfTravelers = booking.NumberOfTravelers,
					PackageId = booking.PackageId,
					TotalBill = booking.TotalBill,
					Status = "Pending",
				};
				_context.UsersPackages.Add(pendingBooking);
			var FrontendUrl = _configuration.GetValue<string>("Front_Url");
			var successUrl = new Uri(new Uri(FrontendUrl), $"tour-packages/booking/confirm-booking/success?bookingId={pendingBooking.Id}").ToString();
			var cancelUrl = new Uri(new Uri(FrontendUrl), "booking/cancel").ToString();
			await _context.SaveChangesAsync();


			// Create Stripe session
			var options = new SessionCreateOptions
			{
				PaymentMethodTypes = new List<string> { "card" },
				LineItems = new List<SessionLineItemOptions>
			{
				new SessionLineItemOptions
				{
					PriceData = new SessionLineItemPriceDataOptions
					{
						Currency = "Pkr",
						ProductData = new SessionLineItemPriceDataProductDataOptions
						{
							Name = "Travel Package",
							Description = $"Package: {package.Title}"
						},
						UnitAmount = (long)(pendingBooking.TotalBill * 100),
					},
					Quantity = 1,
				},
			},
				Mode = "payment",
				SuccessUrl = successUrl,
				CancelUrl = cancelUrl

			};

				var service = new SessionService();
				var session = await service.CreateAsync(options);

				return Ok(new { IsSuccess = true, SessionId = session.Id });
			}

	}
}
