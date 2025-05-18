using IdealTrip.Models;
using IdealTrip.Services;
using Microsoft.AspNetCore.Mvc;

namespace IdealTrip.Controllers
{
	[ApiController]
	[Route("api/contact")]
	public class ContactController : ControllerBase
	{
		private readonly EmailService _emailService;

		public ContactController(EmailService emailService)
		{
			_emailService = emailService;
		}

		[HttpPost("send")]
		public async Task<IActionResult> SendContactMessage([FromBody] ContactUsDto contact)
		{
			if (!ModelState.IsValid) return BadRequest("Invalid input");

			var body = $@"
            <h3>New Contact Us Submission</h3>
            <p><strong>Name:</strong> {contact.FirstName} {contact.LastName}</p>
            <p><strong>Email:</strong> {contact.Email}</p>
            <p><strong>Phone:</strong> {contact.PhoneNumber}</p>
            <p><strong>Company:</strong> {contact.Company}</p>
            <p><strong>Message:</strong> {contact.Message}</p>
        ";

			await _emailService.SendEmailAsync("ideal.trrip@gmail.com", "New Contact Us Message", body);

			return Ok(new UserManagerResponse
			{
				Data = null,
				IsSuccess = true,
				Messege = "Message sent successfully"
			});
		}
	}

}
