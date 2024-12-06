using IdealTrip.Helpers;
using IdealTrip.Models;
using IdealTrip.Models.Login;
using IdealTrip.Models.Register;
using IdealTrip.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdealTrip.Controllers
{
	[Route("api/[controller]")]
	public class AuthController : ControllerBase
	{
		private readonly IUserService _userService;

		public AuthController(IUserService userService)
		{
			_userService = userService;
		}

		#region Registration

		[HttpPost("register/tourist")]
		public async Task<IActionResult> RegisterTourist([FromForm] RegisterTouristModel model)
		{
			try
			{
				if (ModelState.IsValid)
				{
					var result = await _userService.RegisterTouristAsync(model, "Tourist");
					if (result.IsSuccess)
					{
						// Send OTP to user's email
						var emailSent = await _userService.SendEmailVerificationAsync(model.Email);
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
							Messege = "Something went wrong while sending OTP. Please try again.",
							IsSuccess = false
						});
					}
					return BadRequest(result);
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

		[HttpPost("register/transporter")]
		public async Task<IActionResult> RegisterTransporter([FromForm] RegisterTransportorModel model)
		{
			try
			{
				if (ModelState.IsValid)
				{
					var result = await _userService.RegisterTranporterAsync(model, "Transporter");
					if (result.IsSuccess)
					{
						// Send OTP to user's email
						var emailSent = await _userService.SendEmailVerificationAsync(model.Email);
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
							Messege = "Something went wrong while sending OTP. Please try again.",
							IsSuccess = false
						});
					}
					return BadRequest(result);
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
		[HttpPost("register/tourguide")]
		public async Task<IActionResult> RegisterTourGuide([FromForm] RegisterTourGuideModel model)
		{
			try
			{
				if (ModelState.IsValid)
				{
					var result = await _userService.RegisterTourGuideAsync(model, "TourGuide");
					if (result.IsSuccess)
					{
						// Send OTP to user's email
						var emailSent = await _userService.SendEmailVerificationAsync(model.Email);
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
							Messege = "Something went wrong while sending OTP. Please try again.",
							IsSuccess = false
						});
					}
					return BadRequest(result);
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
		[HttpPost("register/localhomeowner")]
		public async Task<IActionResult> RegisterLocalHomeOwner([FromForm] RegisterLocalHomeOwnerModel model)
		{
			try
			{
				if (ModelState.IsValid)
				{
					var result = await _userService.RegisterLocalHomeOwnerAsync(model, "LocalHomeOwner");
					if (result.IsSuccess)
					{
						// Send OTP to user's email
						var emailSent = await _userService.SendEmailVerificationAsync(model.Email);
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
							Messege = "Something went wrong while sending OTP. Please try again.",
							IsSuccess = false
						});
					}
					return BadRequest(result);
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

		[HttpPost("register/hotelowner")]
		public async Task<IActionResult> RegisterHotelOwner([FromForm] RegisterHotelOwnerModel model)
		{
			try
			{
				if (ModelState.IsValid)
				{
					var result = await _userService.RegisterHotelOwnerAsync(model, "HotelOwner");
					if (result.IsSuccess)
					{
						// Send OTP to user's email
						var emailSent = await _userService.SendEmailVerificationAsync(model.Email);
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
							Messege = "Something went wrong while sending OTP. Please try again.",
							IsSuccess = false
						});
					}
					return BadRequest(result);
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
		#region Login

		[HttpPost("login")]
		public async Task<IActionResult> LoginAsync([FromBody] LoginModel model)
		{
			try
			{
				if (ModelState.IsValid)
				{
					var result = await _userService.LoginUserAsync(model);
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
					IsSuccess = false
				});
			}
		}

		[HttpPost("admin-login")]
		public async Task<IActionResult> AdminLogin(LoginModel model)
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
		//[HttpPost("resend-reset-password-link")]
		//public async Task<IActionResult> ResendResetPasswordLink([FromBody] ResendLinkModel model)
		//{
		//	try
		//	{
		//		if (ModelState.IsValid)
		//		{
		//			var emailSent = await _userService.SendPasswordResetLinkAsync(model.email);
		//			if (emailSent)
		//			{
		//				return Ok(new UserManagerResponse
		//				{
		//					Messege = "Link has been resent to your email.",
		//					IsSuccess = true
		//				});
		//			}
		//			return BadRequest(new UserManagerResponse
		//			{
		//				Messege = "Unable to resend Link. Please try again.",
		//				IsSuccess = false
		//			});
		//		}
		//		return BadRequest(new UserManagerResponse
		//		{
		//			Messege = "Invalid request properties.",
		//			IsSuccess = false
		//		});
		//	}
		//	catch (Exception ex)
		//	{
		//		return StatusCode(500, new UserManagerResponse
		//		{
		//			Messege = "Unable to resend Link. Please try again later.",
		//			IsSuccess = false
		//		});
		//	}
		//}


	}
}
