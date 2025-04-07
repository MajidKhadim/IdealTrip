using IdealTrip.Models;
using IdealTrip.Models.Database_Tables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace IdealTrip.Controllers
{
	[ApiController]
	[Route("api/email")]
	public class EmailWebhookController : ControllerBase
	{
		private ApplicationDbContext _context;
		private ILogger<EmailWebhookController> _logger;
		public EmailWebhookController(ApplicationDbContext context, ILogger<EmailWebhookController> logger)
		{
			_context = context;
			_logger = logger;
		}
		[HttpPost("bounce")]
		public async Task<IActionResult> HandleBounce()
		{
			// Read the raw request body as a string
			using (var reader = new StreamReader(Request.Body))
			{
				var requestBody = await reader.ReadToEndAsync();

				// Log the raw request body for debugging
				_logger.LogInformation("Received request body: {RequestBody}", requestBody);

				// Deserialize the request body into a list of BounceEvent objects
				var bounceEvents = JsonConvert.DeserializeObject<List<BounceEvent>>(requestBody);

				// Check if there are bounce events to process
				if (bounceEvents == null || !bounceEvents.Any())
				{
					return BadRequest("No bounce events found.");
				}

				// Process each bounce event
				foreach (var bounceEvent in bounceEvents)
				{
					if (bounceEvent.Event == "bounce" || bounceEvent.Event == "dropped")
					{
						// Log the bounce event for debugging
						_logger.LogInformation("Processing bounce event for email: {Email}", bounceEvent.Email);

						// Save the bounce event to the database (implement SaveBounceAsync)
						await SaveBounceAsync(bounceEvent);
					}
				}
			}

			// Return a generic response after processing the bounce events
			return Ok();
		}

		//}
		//[HttpPost("bounce")]
		//public async Task<IActionResult> HandleBounce()
		//{
		//	// Read the raw request body as a string
		//	using (var reader = new StreamReader(Request.Body))
		//	{
		//		var requestBody = await reader.ReadToEndAsync();

		//		// Log the raw request body
		//		_logger.LogInformation("Received request body: {RequestBody}", requestBody);
		//	}

		//	// Return a generic response for now
		//	return Ok();
		//}


		private async Task SaveBounceAsync(BounceEvent bounceEvent)
		{
			var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == bounceEvent.Email);
			if (user != null)
			{
				user.IsEmailBounced = true;
				user.BounceReason = bounceEvent.Reason;
				await _context.SaveChangesAsync();
			}
		}
	}

	public class BounceEvent
	{
		public string Email { get; set; }
		public string Event { get; set; }
		public string Reason { get; set; }
		public string SgEventId { get; set; }
		public string SgMessageId { get; set; }
		public string SmtpId { get; set; }
		public long Timestamp { get; set; }
	}

}
