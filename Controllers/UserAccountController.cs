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
    [ApiController]
	[Route("api/[controller]")]
	public class UserAccountController : ControllerBase
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly JwtHelper _jwtHelper;
		private readonly ApplicationDbContext _context;
		private readonly EmailService _emailService;

		public UserAccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, JwtHelper jwtHelper, ApplicationDbContext context, EmailService emailService)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_jwtHelper = jwtHelper;
			_context = context;
			_emailService = emailService;
		}
		#region Registration

		[HttpPost("register/admin")]
		public async Task<IActionResult> RegisterAdmin([FromForm] RegisterAdminDto model)
		{
			return await RegisterUser(model, "Admin");
		}

		[HttpPost("register/tourist")]
		public async Task<IActionResult> RegisterTourist([FromForm] RegisterTouristDto model)
		{
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
		private async Task<IActionResult> RegisterUser(RegisterDtoBase model, string role)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var user = new ApplicationUser
			{
				UserName = model.FullName,
				Email = model.Email,
				FullName = model.FullName,
				Address = model.Address,
				Role = role,
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
					return BadRequest("Failed to save profile photo path.");
				}
			}

			// Email Confirmation Logic
			var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
			var confirmationLink = Url.Action("ConfirmEmail", "UserAccount", new { userId = user.Id, token }, Request.Scheme);

			// Send Confirmation Email via SendGrid
			var subject = "Email Confirmation";
			var message = $"Your Email is being registered in Ideal Trip and you are going to be a user as {role}!!!" +
				$"Please confirm your email by clicking the link: <a href='{confirmationLink}'>Confirm Email</a>" +
				$"Regards : Ideal Trip(Make your Journey with Pleasure)";
			var emailSent = await _emailService.SendEmailAsync(user.Email, subject, message);

			if (!emailSent)
				return BadRequest("Failed to send confirmation email.");

			return Ok("Registration successful. Please check your email to confirm your account.");
		}


		private async Task<IActionResult> RegisterUserWithProof(RegisterWithProofDto model, string role)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			// Create a new user
			var user = new ApplicationUser
			{
				UserName = model.FullName,
				Email = model.Email,
				FullName = model.FullName,
				Address = model.Address,
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
			var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
			var confirmationLink = Url.Action("ConfirmEmail", "UserAccount", new { userId = user.Id, token }, Request.Scheme);

			// Send Confirmation Email via SendGrid
			var subject = "Email Confirmation";
			var message = $"Your Email is being registered in Ideal Trip and you are going to be a user as {role}!!!" +
				$"Please confirm your email by clicking the link: <a href='{confirmationLink}'>Confirm Email</a>" +
				$"Regards : Ideal Trip(Make your Journey with Pleasure)";
			var emailSent = await _emailService.SendEmailAsync(user.Email, subject, message);

			if (!emailSent)
				return BadRequest("Failed to send confirmation email.");
			return Ok("Registration successful. Please check your email to confirm your account.");
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

			if (!string.IsNullOrEmpty(model.Address))
			{
				user.Address = model.Address;
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
