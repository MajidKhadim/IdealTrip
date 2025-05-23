﻿using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

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

			 _apiKey = Environment.GetEnvironmentVariable("ENV_SENDGRID_API_KEY");
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
