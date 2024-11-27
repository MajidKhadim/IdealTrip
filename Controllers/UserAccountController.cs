using IdealTrip.Helpers;
using IdealTrip.Models;
using IdealTrip.Models.Enums;
using IdealTrip.Models.Login;
using IdealTrip.Models.Register;
using IdealTrip.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdealTrip.Controllers
{
	[Route("api/[controller]")]
	public class UserAccountController : ControllerBase
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly JwtHelper _jwtHelper;
		private readonly ApplicationDbContext _context;
		private readonly EmailService _emailService;
		private readonly IConfiguration _config;
		private readonly ILogger<UserAccountController> _logger;

		public UserAccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, JwtHelper jwtHelper, ApplicationDbContext context, EmailService emailService, IConfiguration config,ILogger<UserAccountController> logger)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_jwtHelper = jwtHelper;
			_context = context;
			_emailService = emailService;
			_config = config;
			_logger = logger;
			_logger.LogInformation("Logging Started");
		}
		#region Registration

		[HttpPost("register/tourist")]
		public async Task<IActionResult> RegisterTourist([FromBody] RegisterTouristDto model)
		{
			_logger.LogInformation("Recived Data : {Data}", System.Text.Json.JsonSerializer.Serialize(model));
			return await RegisterUser(model, "Tourist");
		}

		[HttpPost("register/localhomeowner")]
		public async Task<IActionResult> RegisterLocalHomeOwner([FromForm] RegisterLocalHomeOwnerDto model)
		{
			return await RegisterUserWithProof(model, "LocalHomeOwner");
		}

		[HttpPost("register/hotelowner")]
		public async Task<IActionResult> RegisterHotelOwner([FromForm] RegisterHotelOwnerDto model)
		{
			return await RegisterUserWithProof(model, "HotelOwner");
		}

		[HttpPost("register/tourguide")]
		public async Task<IActionResult> RegisterTourGuide([FromForm] RegisterTourGuideDto model)
		{
			return await RegisterUserWithProof(model, "TourGuide");
		}
		[HttpPost("register/transporter")]
		public async Task<IActionResult> RegisterTransporter([FromForm] RegisterTourGuideDto model)
		{
			return await RegisterUserWithProof(model, "Transporter");
		}
		private async Task<IActionResult> RegisterUser(RegisterDtoBase model, string role)
		{
			try
			{
				if (!ModelState.IsValid)
					return BadRequest(ModelState);
				var existingUser = await _userManager.FindByEmailAsync(model.Email);
				if (existingUser != null)
				{
					return BadRequest(new { Message = "This email is already associated with an account." });
				}
				if (model.Password != model.ConfirmPassword)
				{
					return BadRequest(new { Messege = "Password and ConfirmPassword should be same" });
				}
				var user = new ApplicationUser
				{
					UserName = model.FullName,
					Email = model.Email,
					FullName = model.FullName,
					Role = role,
					PhoneNumber = model.PhoneNumber,
					Status = role == "Tourist" ? ProofStatus.Verified : ProofStatus.Pending
				};



				var result = await _userManager.CreateAsync(user, model.Password);

				if (!result.Succeeded)
					return BadRequest(result.Errors);
				if (model.ProfilePhoto != null)
				{
					var profilePhotoPath = await SaveFile(model.ProfilePhoto, "profilephotos", user.Id.ToString());
					user.ProfilePhotoPath = profilePhotoPath; // Save relative path to database

					var updateResult = await _userManager.UpdateAsync(user);
					if (!updateResult.Succeeded)
					{
						return BadRequest(new { Messege = "Failed to save profile photo path." });
					}
				}
				else
				{
					user.ProfilePhotoPath = null;
				}

				// Email Confirmation Logic
				// Generate Email Confirmation Token
				var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

				// Generate Confirmation Link
				var baseUrl = _config["BASE_URL"];
				var confirmationLink = $"{baseUrl}/api/auth/confirm-email?userId={user.Id}&token={token}";

				// Email Subject
				var subject = "Welcome to Ideal Trip - Confirm Your Email";

				// Email Content
				var headerText = "Email Confirmation Required";
				var bodyContent = $@"
    <b><p>Hello {user.FullName},</p></b>
    <p>Welcome to <b>Ideal Trip</b>! We are thrilled to have you as a {role} on our platform.</p>
    <p>To get started, please confirm your email address by clicking the button below:</p>";
				var buttonText = "Confirm Email";
				var buttonLink = confirmationLink;

				// Generate Email Body Using Template
				var emailBody = EmailTemplateHelper.GenerateEmailTemplate(headerText, bodyContent, buttonText, buttonLink);
				// Send Email
				var emailSent = await _emailService.SendEmailAsync(user.Email, subject, emailBody);

				if (!emailSent)
					return BadRequest(new { Messege = "Failed to send Confirmation Email." });

				return Ok(new { Messege = "Registration successful! A confirmation email has been sent to your email address. Please check your inbox to confirm your account." });
			}
			catch (Exception ex)
			{
				// Log the exception (you can use a logger here)
				_logger.LogError(ex, "An error occurred while registering the user.");

				// Return a generic error message
				return StatusCode(500, new { Message = "Unable to perform the task now. Please try again later." });
			}
		}


		private async Task<IActionResult> RegisterUserWithProof(RegisterWithProofDto model, string role)
		{
			try { 
			if (!ModelState.IsValid)
				return BadRequest(ModelState);
			var existingUser = await _userManager.FindByEmailAsync(model.Email);
			if (existingUser != null)
			{
				return BadRequest(new { Message = "This email is already associated with an account." });
			}
			// Create a new user
			var user = new ApplicationUser
			{
				UserName = model.FullName,
				Email = model.Email,
				FullName = model.FullName,
				PhoneNumber = model.PhoneNumber,
				Role = role
			};

			// Save user to the database
			var result = await _userManager.CreateAsync(user, model.Password);
			if (!result.Succeeded)
				return BadRequest(result.Errors);

			// Handle Profile Photo Upload (from base class)
			if (model.ProfilePhoto != null)
			{
				var profilePhotoPath = await SaveFile(model.ProfilePhoto, "profilephotos", user.Id.ToString());
				user.ProfilePhotoPath = profilePhotoPath; // Save relative path to database

				var updateResult = await _userManager.UpdateAsync(user);
				if (!updateResult.Succeeded)
				{
					return BadRequest("Failed to save profile photo path.");
				}
			}
			else
			{
				user.ProfilePhotoPath = null;
			}

			// Handle Proof Documents Upload (specific to RegisterWithProofDto)
			if (model.ProofDocuments != null && model.ProofDocuments.Any())
			{
				foreach (var proofDocument in model.ProofDocuments)
				{
					var proofPath = await SaveFile(proofDocument, "proofs", user.Id.ToString());

					// Save proof details to the database
					var proof = new Proof
					{
						UserId = user.Id,
						DocumentType = model.DocumentType, // Optional
						DocumentPath = proofPath // Relative path
					};

					_context.Proofs.Add(proof);
				}

				await _context.SaveChangesAsync();
			}

			// Email Confirmation Logic
			// Generate Email Confirmation Token
			var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

			// Generate Confirmation Link
			var baseUrl = _config["BASE_URL"];
			var confirmationLink = $"{baseUrl}/api/auth/confirm-email?userId={user.Id}&token={token}";

			// Email Subject
			var subject = "Welcome to Ideal Trip - Confirm Your Email";

			// Email Content
			var headerText = "Email Confirmation Required";
			var bodyContent = $@"
    <p>Hello {user.FullName},</p>
    <p>Welcome to <b>Ideal Trip</b>! We are thrilled to have you as a {role} on our platform.</p>
    <p>To get started, please confirm your email address by clicking the button below:</p>";
			var buttonText = "Confirm Email";
			var buttonLink = confirmationLink;

			// Generate Email Body Using Template
			var emailBody = EmailTemplateHelper.GenerateEmailTemplate(headerText, bodyContent, buttonText, buttonLink);

			// Send Email
			var emailSent = await _emailService.SendEmailAsync(user.Email, subject, emailBody);

			if (!emailSent)
				return BadRequest("Failed to send confirmation email.");

			return Ok("Registration successful! A confirmation email has been sent to your email address. Please check your inbox to confirm your account.");
			}
			catch (Exception ex)
			{
				// Log the exception (you can use a logger here)
				_logger.LogError(ex, "An error occurred while registering the user.");

				// Return a generic error message
				return StatusCode(500, new { Message = "Unable to perform the task now. Please try again later." });
			}
		}


		[HttpGet("confirm-email")]
		public async Task<IActionResult> ConfirmEmail(string userId, string token)
		{
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
				return BadRequest("Invalid user ID.");

			var result = await _userManager.ConfirmEmailAsync(user, token);
			if (!result.Succeeded)
				return BadRequest("Email confirmation failed.");

			return Ok("Email confirmed successfully.");
		}

		#endregion

		#region Login

		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginDto model)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var user = await _userManager.FindByEmailAsync(model.Email);
			if (user == null)
				return Unauthorized("Invalid email or password.");

			if (!await _userManager.IsEmailConfirmedAsync(user))
				return Unauthorized("Email not confirmed. Please check your inbox.");

			var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, false, false);
			if (!result.Succeeded)
				return Unauthorized("Invalid email or password.");

			// Generate JWT Token
			var token = _jwtHelper.GenerateToken(user.Id.ToString(), user.Email, user.Role);

			return Ok(new { Token = token });
		}


		#endregion

		#region Password

		[HttpPost("forgot-password")]
		public async Task<IActionResult> ForgotPassword([FromBody] ForgetPasswordDto model)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var user = await _userManager.FindByEmailAsync(model.EmailAddress);
			if (user == null)
				return BadRequest("Invalid email or email not confirmed.");

			// Generate password reset token
			var token = await _userManager.GeneratePasswordResetTokenAsync(user);
			var resetLink = Url.Action("ResetPassword", "UserAccount", new { token, email = user.Email }, Request.Scheme);

			// Send email
			var subject = "Password Reset Request";

			var headerText = "Ideal Trip";
			var bodyContent = @"
    <p>Dear User,</p>
    <p>We received a request to reset your password for your Ideal Trip account. If you made this request, please click the button below to reset your password:</p>";
			var buttonText = "Reset Password";
			var buttonLink = resetLink;

			var emailBody = EmailTemplateHelper.GenerateEmailTemplate(headerText, bodyContent, buttonText, buttonLink);

			var emailSent = await _emailService.SendEmailAsync(user.Email, subject, emailBody);

			if (!emailSent)
				return BadRequest("Failed to send reset password email.");

			return Ok("Password reset link has been sent to your email.");
		}

		[HttpPost("reset-password")]
		public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var user = await _userManager.FindByEmailAsync(model.Email);
			if (user == null)
				return BadRequest("Invalid email.");

			// Verify the token and reset the password
			var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
			if (!result.Succeeded)
				return BadRequest(result.Errors);

			return Ok("Password has been reset successfully.");
		}


		#endregion

		[HttpPost("update-user/{userId}")]
		public async Task<IActionResult> UpdateUser(string userId, [FromForm] UpdateUserDto model)
		{
			// Find the user by ID
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
				return NotFound(new { Message = "User not found." });
			}

			// Update fields if they are provided
			if (!string.IsNullOrEmpty(model.FullName))
			{
				user.UserName = model.FullName;
			}

			if (model.ProfilePhoto != null)
			{
				// Delete the old profile photo if it exists
				if (!string.IsNullOrEmpty(user.ProfilePhotoPath))
				{
					var oldProfilePhotoPath = Path.Combine("wwwroot", user.ProfilePhotoPath);
					if (System.IO.File.Exists(oldProfilePhotoPath))
					{
						System.IO.File.Delete(oldProfilePhotoPath);
					}
				}

				// Save the new profile photo
				var profilePhotoPath = await SaveFile(model.ProfilePhoto, "profilephotos", user.Id.ToString());
				user.ProfilePhotoPath = profilePhotoPath; // Save the relative path in the database
			}

			// Save the updated user
			var result = await _userManager.UpdateAsync(user);
			if (!result.Succeeded)
			{
				return BadRequest(new { Message = "Failed to update user.", Errors = result.Errors });
			}

			return Ok(new { Message = "User updated successfully.", ProfilePhotoPath = user.ProfilePhotoPath });
		}
		#region UsersInfo
		
		#endregion




		private async Task<string> SaveFile(IFormFile file, string directory, string userId)
		{
			// Create a user-specific directory
			var userDirectory = Path.Combine("wwwroot", directory, userId);
			if (!Directory.Exists(userDirectory))
			{
				Directory.CreateDirectory(userDirectory);
			}

			// Generate a unique file name
			var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
			var fullPath = Path.Combine(userDirectory, fileName);

			// Save the file to the directory
			using (var stream = new FileStream(fullPath, FileMode.Create))
			{
				await file.CopyToAsync(stream);
			}

			// Return relative path for database storage
			return Path.Combine(directory, userId, fileName);
		}

	}
}
