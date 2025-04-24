using IdealTrip.Helpers;
using IdealTrip.Models;
using IdealTrip.Models.Login;
using IdealTrip.Models.Register;
using IdealTrip.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace IdealTrip.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class AuthController : ControllerBase
	{
		private readonly IUserService _userService;
		private readonly IHttpContextAccessor _contextAccessor;
		private ApplicationDbContext _context;
		private readonly EmailValidationService _emailValidationService;

		public AuthController(IUserService userService,IHttpContextAccessor contextAccessor,ApplicationDbContext context,EmailValidationService emailValidationService)
		{
			_userService = userService;
			_contextAccessor = contextAccessor;
			_context = context;
			_emailValidationService = emailValidationService;
		}

		#region Registration

		[HttpPost("register/tourist")]
		public async Task<IActionResult> RegisterTourist([FromForm] RegisterTouristModel model)
		{
			try
			{
				if (ModelState.IsValid)
				{
					//var isRealEmail = await _emailValidationService.IsEmailRealAsync(model.Email);
					//if (!isRealEmail)
					//{
					//	return BadRequest(new UserManagerResponse
					//	{
					//		Messege = "The email address you entered does not appear to exist. Please enter a valid email.",
					//		IsSuccess = false,
					//		Errors = new List<string>() { "The email address you entered does not appear to exist. Please enter a valid email." }
					//	});
					//}
					var result = await _userService.RegisterTouristAsync(model, "Tourist");
					if (result.IsSuccess)
					{

						// Send OTP to user's email
						var emailSent = await _userService.SendEmailVerificationAsync(model.Email);

						// 3. Check if the email bounced
						var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
						if (user?.IsEmailBounced == true)
						{
							string bounceReason = user.BounceReason ?? "Unknown";
							await _userService.DeleteUser(model.Email);

							return BadRequest(new UserManagerResponse
							{
								Messege = $"Email verification failed. Reason: {bounceReason}",
								IsSuccess = false,
								Errors = new List<string> { $"Email verification failed. Reason: {bounceReason}" }
							});
						}

						if (emailSent)
						{
							return Ok(new UserManagerResponse
							{
								Messege = "Registration successful! Check your Email for Verification Link",
								IsSuccess = true
							});
						}
						// If OTP fails to send, delete the user and notify the client
						await _userService.DeleteUser(model.Email);
						return BadRequest(new UserManagerResponse
						{
							Messege = "Something went wrong while sending Verification Email. Please try again.",
							IsSuccess = false,
							Errors = new List<string> { "Verification email sending failed." }
						});
					}
					return BadRequest(result);
				}
				var errors = ModelState.Values
			.SelectMany(v => v.Errors)
			.Select(e => e.ErrorMessage)
			.ToList();

				return BadRequest(new UserManagerResponse
				{
					Messege = "Some properties are not valid",
					IsSuccess = false,
					Errors = errors
				});
			}
			catch (Exception ex)
			{
				
				return StatusCode(500, new UserManagerResponse
				{
					Messege = "Unable to perform the task now. Please try again later.",
					IsSuccess = false,
					Errors = new List<string> { "Unable to perform the task now. Please try again later." }
				});
			}
		}

		[HttpPost("register/transporter")]
		public async Task<IActionResult> RegisterTransporter([FromForm] RegisterTransportorModel model)
		{
			try
			{
				if (ModelState.IsValid)
				{
					var isRealEmail = await _emailValidationService.IsEmailRealAsync(model.Email);
					if (!isRealEmail)
					{
						return BadRequest(new UserManagerResponse
						{
							Messege = "The email address you entered does not appear to exist. Please enter a valid email.",
							IsSuccess = false,
							Errors = new List<string>() { "The email address you entered does not appear to exist. Please enter a valid email." }
						});
					}
					var result = await _userService.RegisterTranporterAsync(model, "Transporter");
					if (result.IsSuccess)
					{
						// Send OTP to user's email
						var emailSent = await _userService.SendEmailVerificationAsync(model.Email);
						var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
						if (user?.IsEmailBounced == true)
						{
							string bounceReason = user.BounceReason ?? "Unknown";
							await _userService.DeleteUser(model.Email);

							return BadRequest(new UserManagerResponse
							{
								Messege = $"Email verification failed. Reason: {bounceReason}",
								IsSuccess = false,
								Errors = new List<string> { $"Email verification failed. Reason: {bounceReason}" }
							});
						}
						if (emailSent)
						{
							return Ok(new UserManagerResponse
							{
								Messege = "Registration successful! Check your Email for Verification Link",
								IsSuccess = true
							});
						}

						// If OTP fails to send, delete the user and notify the client
						await _userService.DeleteUser(model.Email);
						return BadRequest(new UserManagerResponse
						{
							Messege = "Something went wrong while sending Verification Email. Please try again.",
							IsSuccess = false,
							Errors = new List<string> { "Verification email sending failed." }
						});
					}
					return BadRequest(result);
				}
				var errors = ModelState.Values
			.SelectMany(v => v.Errors)
			.Select(e => e.ErrorMessage)
			.ToList();

				return BadRequest(new UserManagerResponse
				{
					Messege = "Some properties are not valid",
					IsSuccess = false,
					Errors = errors
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new UserManagerResponse
				{
					Messege = "Unable to perform the task now. Please try again later.",
					IsSuccess = false,
					Errors = new List<string> { "Unable to perform the task now. Please try again later." }
				});
			}
		}
		[HttpPost("register/tourguide")]
		public async Task<IActionResult> RegisterTourGuide([FromForm] RegisterTourGuideModel model)
		{
			try
			{
				if (ModelState.IsValid)
				{
					var isRealEmail = await _emailValidationService.IsEmailRealAsync(model.Email);
					if (!isRealEmail)
					{
						return BadRequest(new UserManagerResponse
						{
							Messege = "The email address you entered does not appear to exist. Please enter a valid email.",
							IsSuccess = false,
							Errors = new List<string>() { "The email address you entered does not appear to exist. Please enter a valid email." }
						});
					}
					var result = await _userService.RegisterTourGuideAsync(model, "TourGuide");
					if (result.IsSuccess)
					{
						// Send OTP to user's email
						var emailSent = await _userService.SendEmailVerificationAsync(model.Email);
						var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
						if (user?.IsEmailBounced == true)
						{
							string bounceReason = user.BounceReason ?? "Unknown";
							await _userService.DeleteUser(model.Email);

							return BadRequest(new UserManagerResponse
							{
								Messege = $"Email verification failed. Reason: {bounceReason}",
								IsSuccess = false,
								Errors = new List<string> { $"Email verification failed. Reason: {bounceReason}" }
							});
						}
						if (emailSent)
						{
							return Ok(new UserManagerResponse
							{
								Messege = "Registration successful! Check your Email for Verification Link",
								IsSuccess = true
							});
						}

						// If OTP fails to send, delete the user and notify the client
						await _userService.DeleteUser(model.Email);
						return BadRequest(new UserManagerResponse
						{
							Messege = "Something went wrong while sending Verification Email. Please try again.",
							IsSuccess = false,
							Errors = new List<string> { "Verification email sending failed." }
						});
					}
					return BadRequest(result);
				}
				var errors = ModelState.Values
			.SelectMany(v => v.Errors)
			.Select(e => e.ErrorMessage)
			.ToList();

				return BadRequest(new UserManagerResponse
				{
					Messege = "Some properties are not valid",
					IsSuccess = false,
					Errors = errors
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new UserManagerResponse
				{
					Messege = "Unable to perform the task now. Please try again later.",
					IsSuccess = false,
					Errors = new List<string> { "Unable to perform the task now. Please try again later." }	
				});
			}
		}
		[HttpPost("register/localhomeowner")]
		public async Task<IActionResult> RegisterLocalHomeOwner([FromForm] RegisterLocalHomeOwnerModel model)
		{
			try
			{
				if (ModelState.IsValid)
				{
					var isRealEmail = await _emailValidationService.IsEmailRealAsync(model.Email);
					if (!isRealEmail)
					{
						return BadRequest(new UserManagerResponse
						{
							Messege = "The email address you entered does not appear to exist. Please enter a valid email.",
							IsSuccess = false,
							Errors = new List<string>() { "The email address you entered does not appear to exist. Please enter a valid email." }
						});
					}
					var result = await _userService.RegisterLocalHomeOwnerAsync(model, "LocalHomeOwner");
					if (result.IsSuccess)
					{
						// Send OTP to user's email
						var emailSent = await _userService.SendEmailVerificationAsync(model.Email);
						var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
						if (user?.IsEmailBounced == true)
						{
							string bounceReason = user.BounceReason ?? "Unknown";
							await _userService.DeleteUser(model.Email);

							return BadRequest(new UserManagerResponse
							{
								Messege = $"Email verification failed. Reason: {bounceReason}",
								IsSuccess = false,
								Errors = new List<string> { $"Email verification failed. Reason: {bounceReason}" }
							});
						}
						if (emailSent)
						{
							return Ok(new UserManagerResponse
							{
								Messege = "Registration successful! Check your Email for Verification Link",
								IsSuccess = true
							});
						}

						// If OTP fails to send, delete the user and notify the client
						await _userService.DeleteUser(model.Email);
						return BadRequest(new UserManagerResponse
						{
							Messege = "Something went wrong while sending Verification Email. Please try again.",
							IsSuccess = false,
							Errors = new List<string> { "Verification email sending failed." }
						});
					}
					return BadRequest(result);
				}
				var errors = ModelState.Values
			.SelectMany(v => v.Errors)
			.Select(e => e.ErrorMessage)
			.ToList();

				return BadRequest(new UserManagerResponse
				{
					Messege = "Some properties are not valid",
					IsSuccess = false,
					Errors = errors
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new UserManagerResponse
				{
					Messege = "Unable to perform the task now. Please try again later.",
					IsSuccess = false,
					Errors = new List<string> { "Unable to perform the task now. Please try again later." }
				});
			}
		}

		[HttpPost("register/hotelowner")]
		public async Task<IActionResult> RegisterHotelOwner([FromForm] RegisterHotelOwnerModel model)
		{
			try
			{
				if (ModelState.IsValid)
				{
					var isRealEmail = await _emailValidationService.IsEmailRealAsync(model.Email);
					if (!isRealEmail)
					{
						return BadRequest(new UserManagerResponse
						{
							Messege = "The email address you entered does not appear to exist. Please enter a valid email.",
							IsSuccess = false,
							Errors = new List<string>() { "The email address you entered does not appear to exist. Please enter a valid email." }
						});
					}
					var result = await _userService.RegisterHotelOwnerAsync(model, "HotelOwner");
					if (result.IsSuccess)
					{
						// Send OTP to user's email
						var emailSent = await _userService.SendEmailVerificationAsync(model.Email);

						var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
						if (user?.IsEmailBounced == true)
						{
							string bounceReason = user.BounceReason ?? "Unknown";
							await _userService.DeleteUser(model.Email);

							return BadRequest(new UserManagerResponse
							{
								Messege = $"Email verification failed. Reason: {bounceReason}",
								IsSuccess = false,
								Errors = new List<string> { $"Email verification failed. Reason: {bounceReason}" }
							});
						}
						if (emailSent)
						{
							return Ok(new UserManagerResponse
							{
								Messege = "Registration successful! Check your Email for Verification Link",
								IsSuccess = true
							});
						}

						// If OTP fails to send, delete the user and notify the client
						await _userService.DeleteUser(model.Email);
						return BadRequest(new UserManagerResponse
						{
							Messege = "Something went wrong while sending Verification Email. Please try again.",
							IsSuccess = false,
							Errors = new List<string> { "Verification email sending failed." }
						});
					}
					return BadRequest(result);
				}
				var errors = ModelState.Values
			.SelectMany(v => v.Errors)
			.Select(e => e.ErrorMessage)
			.ToList();

				return BadRequest(new UserManagerResponse
				{
					Messege = "Some properties are not valid",
					IsSuccess = false,
					Errors = errors
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new UserManagerResponse
				{
					Messege = "Unable to perform the task now. Please try again later.",
					IsSuccess = false,
					Errors = new List<string> { "Unable to perform the task now. Please try again later." }
				});
			}
		}
		#endregion
		#region Login

		[HttpPost("login")]
		public async Task<IActionResult> LoginAsync([FromBody] LoginModel model)
		{
			try
			{
				if (ModelState.IsValid)
				{
					var result = await _userService.LoginUserAsync(model);
					if (await _userService.IsAdmin(model.Email))
					{
						return BadRequest(new UserManagerResponse
						{
							IsSuccess = false,
							Messege = "Admins are not allowed to Login Here"
						});
					}
					if (result.IsSuccess)
					{
						return Ok(result);
					}
					return BadRequest(result);
				}
				return BadRequest(new UserManagerResponse
				{
					Messege = "Some properties are not valid",
					IsSuccess = false
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new UserManagerResponse
				{
					Messege = "Unable to perform the task now. Please try again later.",
					IsSuccess = false,
					Errors = new List<string> { "Unable to perform the task now. Please try again later." }
				});
			}
		}

		[HttpPost("admin-login")]
		public async Task<IActionResult> AdminLogin([FromBody] LoginModel model)
		{
			try { 
			if (ModelState.IsValid)
			{
				var result = await _userService.LoginUserAsync(model);
				if (result.IsSuccess)
				{
					if (await _userService.IsAdmin(model.Email))
					{
						return Ok(result);
					}
					else
					{
						return BadRequest(new UserManagerResponse { IsSuccess = false, Messege = "Access denied: Only Admins can log in here" });
					}
				}
				else
				{
					BadRequest(result);
				}
			}
			return BadRequest(new UserManagerResponse { Messege = "Some properties are not valid", IsSuccess = false });
		}
			catch (Exception ex)
			{
				return StatusCode(500, new UserManagerResponse
				{
					Messege = "Unable to perform the task now. Please try again later.",
					IsSuccess = false
				});
			}
		}
	#endregion
	#region Password Management
	[HttpPost("forgot-password")]
		public async Task<IActionResult> ForgotPassword([FromBody] ForgetPasswordModel model)
		{
			try
			{
				if (ModelState.IsValid)
				{
					var result = await _userService.SendEmailVerificationAsync(model.EmailAddress);
					if (result)
					{
						return Ok(new UserManagerResponse
						{
							Messege = "Password reset link has been sent to your email.",
							IsSuccess = true
						});
					}
					return BadRequest(new UserManagerResponse
					{
						Messege = "Failed to reset Link.. Please Try Again Later",
						IsSuccess = false
					});
				}
				return BadRequest(new UserManagerResponse
				{
					Messege = "Invalid request properties.",
					IsSuccess = false
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new UserManagerResponse
				{
					Messege = "Unable to process your request. Please try again later.",
					IsSuccess = false
				});
			}
		}

		[HttpPost("reset-password")]
		public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
		{
			if (ModelState.IsValid)
			{
				var result = await _userService.ResetPasswordAsync(model);
				if (result.IsSuccess)
				{
					return Ok(result);
				}
				return BadRequest(result);
			}

			return BadRequest(new UserManagerResponse
			{
				Messege = "Invalid request properties.",
				IsSuccess = false
			});
		}
		#endregion
		[HttpGet("confirm-email")]
		public async Task<IActionResult> ConfirmEmail(string userId, string token)
		{
			if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userId))
				return BadRequest(new UserManagerResponse
				{
					IsSuccess = false,
					Messege = "Invalid email confirmation request."
				}
			);
			var result = await _userService.ConfirmEmail(userId, token);
			if (result.IsSuccess)
			{
				return Redirect(result.Messege);
			}
			return BadRequest(result);
		}
		[HttpPost("resend-link")]
		public async Task<IActionResult> ResendLink([FromBody] ResendLinkModel model)
		{
			try
			{
				if (ModelState.IsValid)
				{
					var emailSent = await _userService.SendEmailVerificationAsync(model.email);
					if (emailSent)
					{
						return Ok(new UserManagerResponse
						{
							Messege = "Link has been resent to your email.",
							IsSuccess = true
						});
					}
					return BadRequest(new UserManagerResponse
					{
						Messege = "Unable to resend Link. Please try again.",
						IsSuccess = false
					});
				}
				return BadRequest(new UserManagerResponse
				{
					Messege = "Invalid request properties.",
					IsSuccess = false
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new UserManagerResponse
				{
					Messege = "Unable to resend Link. Please try again later.",
					IsSuccess = false
				});
			}
		}
		[HttpGet("email-status")]
		public async Task<IActionResult> GetEmailStatus(string email)
		{
			var user = await _userService.GetEmailStatus(email);
			if (user == null) return NotFound();

			return Ok(new
			{
				isConfirmed = user.EmailConfirmed,
				isBounced = user.IsEmailBounced,
				reason = user.BounceReason
			});
		}


	}
}
