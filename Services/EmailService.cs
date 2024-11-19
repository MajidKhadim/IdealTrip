using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Net.Mail;
using System;

namespace IdealTrip.Services
{
	public class EmailService
	{
		private readonly string _apiKey;
		private readonly string _senderEmail;
		private readonly string _senderName;

		public EmailService(IConfiguration configuration)
		{
			var sendGridConfig = configuration.GetSection("SendGrid");
			_apiKey = sendGridConfig["ApiKey"];
			_senderEmail = sendGridConfig["SenderEmail"];
			_senderName = sendGridConfig["SenderName"];
		}

		public async Task<bool> SendEmailAsync(string recipientEmail, string subject, string message)
		{
			var client = new SendGridClient(_apiKey);
			var from = new EmailAddress(_senderEmail, _senderName);
			var to = new EmailAddress(recipientEmail);
			var msg = MailHelper.CreateSingleEmail(from, to, subject, message, message);

			var response = await client.SendEmailAsync(msg);
			return response.StatusCode == System.Net.HttpStatusCode.Accepted;
		}
	}
}
