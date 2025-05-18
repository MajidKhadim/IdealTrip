using Stripe;
using Stripe.Checkout;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace IdealTrip.Services
{
	public class PaymentService
	{
		private readonly string _secretKey;

		public PaymentService(IConfiguration configuration)
		{
			string _secretKey = Environment.GetEnvironmentVariable("SecretKey");
			StripeConfiguration.ApiKey = _secretKey;
		}

		public static async Task<string> CreateCheckoutSession(Guid bookingId, string productName, decimal amount, string currency, string successUrl, string cancelUrl)
		{
			var options = new SessionCreateOptions
			{
				PaymentMethodTypes = new List<string> { "card" },
				LineItems = new List<SessionLineItemOptions>
				{
					new SessionLineItemOptions
					{
						PriceData = new SessionLineItemPriceDataOptions
						{
							Currency = currency,
							UnitAmount = (long)(amount), // Convert to cents
                            ProductData = new SessionLineItemPriceDataProductDataOptions
							{
								Name = productName
							}
						},
						Quantity = 1
					}
				},
				Mode = "payment",
				SuccessUrl = $"{successUrl}?bookingId={bookingId}", // Include product ID for reference
				CancelUrl = cancelUrl
			};

			var service = new SessionService();
			Session session = await service.CreateAsync(options);

			return session.Url; // Returns the Stripe Checkout URL
		}
		public  async Task<object> CreatePaymentIntent(Guid bookingId,string productType,decimal amount)
		{
			if (amount <= 0)
			{
				return new { message = "Invalid amount" };
			}

			var options = new PaymentIntentCreateOptions
			{
				Amount = (long)(amount * 100), // Convert to cents
				Currency = "pkr",
				PaymentMethodTypes = new List<string> { "card" },
				Metadata = new Dictionary<string, string>
			{
				{ "BookingId", bookingId.ToString() },
				{ "ProductType", productType }
			}
			};

			var service = new PaymentIntentService();
			var paymentIntent = await service.CreateAsync(options);

			return new { clientSecret = paymentIntent.ClientSecret };
		}
	}
}
