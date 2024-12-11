using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;

namespace IdealTrip.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public PaymentController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("create-checkout-session")]
        public IActionResult CreateCheckoutSession()
        {
            // Retrieve the Frontend URL from app configuration
            var frontendUrl = _configuration.GetValue<string>("Front_Url");

            if (string.IsNullOrEmpty(frontendUrl))
            {
                return BadRequest(new { message = "Frontend URL is not configured." });
            }

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = 3500000, // Amount in the smallest currency unit (e.g., 35,000 PKR = 3500000 paisa)
                            Currency = "pkr",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Tour Package"
                            }
                        },
                        Quantity = 1
                    }
                },
                Mode = "payment",
                SuccessUrl = $"{frontendUrl}/success?session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{frontendUrl}/cancel"
            };

            var service = new SessionService();
            Session session = service.Create(options);

            return Ok(new { Url = session.Url });
        }

        [HttpGet("verify-session")]
        public IActionResult VerifySession(string sessionId)
        {
            var service = new SessionService();
            var session = service.Get(sessionId);

            // Check payment status
            if (session.PaymentStatus == "paid")
            {
                // Process successful payment (e.g., mark order as paid)
                return Ok(new { message = "Payment Successful" });
            }

            return BadRequest(new { message = "Payment Failed" });
        }
    }
}
