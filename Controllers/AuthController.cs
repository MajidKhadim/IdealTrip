using IdealTrip.Helpers;
using IdealTrip.Models;
using IdealTrip.Models.Login;
using IdealTrip.Models.Register;
using IdealTrip.Services;
using Microsoft.AspNetCore.Mvc;

namespace IdealTrip.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private readonly IUserService _userService;

		public AuthController(IUserService userService)
		{
			_userService = userService;
		}

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
						var otpSent = await _userService.SendOtpAsync(model.Email);
						if (otpSent)
						{
							return Ok(new UserManagerResponse
							{
								Messege = "Registration successful! OTP has been sent to your email.",
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
						var otpSent = await _userService.SendOtpAsync(model.Email);
						if (otpSent)
						{
							return Ok(new UserManagerResponse
							{
								Messege = "Registration successful! OTP has been sent to your email.",
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
						var otpSent = await _userService.SendOtpAsync(model.Email);
						if (otpSent)
						{
							return Ok(new UserManagerResponse
							{
								Messege = "Registration successful! OTP has been sent to your email.",
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
						var otpSent = await _userService.SendOtpAsync(model.Email);
						if (otpSent)
						{
							return Ok(new UserManagerResponse
							{
								Messege = "Registration successful! OTP has been sent to your email.",
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
						var otpSent = await _userService.SendOtpAsync(model.Email);
						if (otpSent)
						{
							return Ok(new UserManagerResponse
							{
								Messege = "Registration successful! OTP has been sent to your email.",
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

		[HttpPost("verify-otp")]
		public async Task<ActionResult<UserManagerResponse>> VerifyOtp([FromBody] VerifyOTPModel model)
		{
			try
			{
				if (ModelState.IsValid)
				{
					var isOtpValid = await _userService.VerifyOtp(model.email, model.Otp);
					if (isOtpValid)
					{
						return Ok(new UserManagerResponse
						{
							Messege = "OTP verified successfully!",
							IsSuccess = true
						});
					}

					return BadRequest(new UserManagerResponse
					{
						Messege = "Invalid or expired OTP. Please try again.",
						IsSuccess = false
					});
				}
				return BadRequest(new UserManagerResponse { Messege = "Invalid request properties.", IsSuccess = false });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new UserManagerResponse
				{
					Messege = "Unable to verify OTP. Please try again later.",
					IsSuccess = false
				});
			}
		}

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

		[HttpPost("resend-otp")]
		public async Task<IActionResult> ResendOtp([FromBody] ResendOtpModel model)
		{
			try
			{
				if (ModelState.IsValid)
				{
					var otpSent = await _userService.SendOtpAsync(model.email);
					if (otpSent)
					{
						return Ok(new UserManagerResponse
						{
							Messege = "OTP has been resent to your email.",
							IsSuccess = true
						});
					}
					return BadRequest(new UserManagerResponse
					{
						Messege = "Unable to resend OTP. Please try again.",
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
					Messege = "Unable to resend OTP. Please try again later.",
					IsSuccess = false
				});
			}
		}

		[HttpPost("forgot-password")]
		public async Task<IActionResult> ForgotPassword([FromBody] ForgetPasswordModel model)
		{
			try
			{
				if (ModelState.IsValid)
				{
					var result = await _userService.ForgotPasswordAsync(model.EmailAddress);
					if (result.IsSuccess)
					{
						return Ok(new UserManagerResponse
						{
							Messege = "Password reset link has been sent to your email.",
							IsSuccess = true
						});
					}
					return BadRequest(result);
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
		public async Task<ActionResult<UserManagerResponse>> ResetPassword([FromBody] ResetPasswordModel model)
		{
			try
			{
				if (ModelState.IsValid)
				{
					if(model.NewPassword != model.ConfirmPassword)
					{
						return new UserManagerResponse
						{
							Messege = "New password and confirm password mismatch",
							IsSuccess = false
						};
					}
					var result = await _userService.ResetPasswordAsync(model.Email,model.Token,model.NewPassword);
					if (result.IsSuccess)
					{
						return Ok(new UserManagerResponse
						{
							Messege = "Password reset successfully.",
							IsSuccess = true
						});
					}
					return BadRequest(result);
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
	}
}
