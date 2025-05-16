//using Azure.Core;
//using Azure.Storage.Blobs.Models;
//using Azure.Storage.Blobs;
using IdealTrip.Helpers;
using IdealTrip.Models;
using IdealTrip.Models.Enums;
using IdealTrip.Models.Login;
using IdealTrip.Models.Register;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Identity.UI.Services;
using IdealTrip.Models.TourGuide_Booking;
using Microsoft.EntityFrameworkCore;
using Stripe.Terminal;
using Stripe;

namespace IdealTrip.Services
{
	public interface IUserService
	{
		Task<UserManagerResponse> RegisterTranporterAsync(RegisterTransportorModel model, string role);
		Task<UserManagerResponse> RegisterTouristAsync(RegisterTouristModel model, string role);
		Task<UserManagerResponse> RegisterLocalHomeOwnerAsync(RegisterLocalHomeOwnerModel model, string role);
		Task<UserManagerResponse> RegisterTourGuideAsync(RegisterTourGuideModel model, string role);
		Task<UserManagerResponse> RegisterHotelOwnerAsync(RegisterHotelOwnerModel model, string role);
		Task<UserManagerResponse> LoginUserAsync(LoginModel model);
		Task<UserManagerResponse> DeleteUser(string userEmail);
		Task<UserManagerResponse> UpdateUser(UpdateUserModel model,string Id);
		Task<UserManagerResponse> ResetPasswordAsync(ResetPasswordModel model);
		Task<bool> IsAdmin(string email);
		Task<UserManagerResponse> GetUserInfo(string userId);
		Task<object> GetTourGuideDetails(string userId);
		Task<UserManagerResponse> GetUserDetails(string userId);
		Task<UserManagerResponse> ConfirmEmail(string userId,string token);
		public Task<bool> SendEmailVerificationAsync(string email);
		Task<UserManagerResponse> SendAccountApprovedEmail(string email);
		Task<UserManagerResponse> SendAccountRejectedEmail(string email);
		Task<ApplicationUser?> GetEmailStatus(string email);

		Task<UserManagerResponse> GetUsersAllBookings(string userId);
		public Task<UserManagerResponse> GetUserBookingDetails(string bookingType, string bookingId);
	}

	public class UserService : IUserService
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly ApplicationDbContext _context;
		private readonly EmailService _emailService;
		private readonly IConfiguration _config;
		private readonly ILogger<UserService> _logger;
		public readonly JwtHelper _JwtHelper;
		//private readonly BlobServiceClient _blobServiceClient;
		private readonly string _profilePhotoContainerName = "profilepictures";
		private readonly string _proofContainerName = "proofs";
		private readonly IHttpContextAccessor _httpContextAccessor;

		public UserService(
			UserManager<ApplicationUser> userManager,
			ApplicationDbContext context,
			EmailService emailService,
			IConfiguration config,
			ILogger<UserService> logger,
			JwtHelper jwtHelper,
			IHttpContextAccessor httpContextAccessor)
		{
			_userManager = userManager;
			_context = context;
			_emailService = emailService;
			_config = config;
			_logger = logger;
			_JwtHelper = jwtHelper;
			_httpContextAccessor = httpContextAccessor;
		}

		#region Registration

		public async Task<UserManagerResponse> RegisterTranporterAsync(RegisterTransportorModel model, string role)
		{
			try
			{
				if (model == null) throw new ArgumentNullException(nameof(model));

				var existingUser = await _userManager.FindByEmailAsync(model.Email);
				if (existingUser != null)
				{
					return new UserManagerResponse
					{
						IsSuccess = false,
						Messege = "This Email is already associated with another Account",
						Errors = new List<string> { "This Email is already associated with another Account" }
					};
				}

				if (model.Password != model.ConfirmPassword)
				{
					return new UserManagerResponse
					{
						IsSuccess = false,
						Messege = "Confirm Password does not match the password",
						Errors = new List<string> { "Confirm Password does not match the password" }
					};
				}

				var user = new ApplicationUser
				{
					UserName = model.FullName,
					Email = model.Email,
					FullName = model.FullName,
					Role = role,
					PhoneNumber = model.PhoneNumber,
					Address = model.Address,
					Status = ProofStatus.Pending,
					IsDeleted = false
				};

				var result = await _userManager.CreateAsync(user, model.Password);
				if (!result.Succeeded)
				{
					return new UserManagerResponse
					{
						IsSuccess = false,
						Messege = "User registration failed",
						Errors = result.Errors.Select(e => e.Description)
					};
				}

				await _userManager.AddToRoleAsync(user, role);

				// Save profile photo
				if (model.ProfilePhoto != null)
				{
					var saveResult = await SaveFileWithValidation(user.Id, "profilephotos", model.ProfilePhoto);
					if (!saveResult.IsSuccess)
					{
						await _userManager.DeleteAsync(user);
						return new UserManagerResponse { IsSuccess = false, Messege = saveResult.Messege };
					}
					user.ProfilePhotoPath = saveResult.Path;
					await _userManager.UpdateAsync(user);
				}

				// Save proofs
				if (model.VehicleRegistrationDoc != null)
					await SaveProof(user.Id, "Vehicle Registration", model.VehicleRegistrationDoc);

				if (model.DriverLicense != null)
					await SaveProof(user.Id, "Driver License", model.DriverLicense);

				return new UserManagerResponse { IsSuccess = true, Messege = "User registered successfully." };
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while registering transporter.");
				var user = await _userManager.FindByEmailAsync(model.Email);
				await _userManager.DeleteAsync(user);
				return new UserManagerResponse
				{
					IsSuccess = false,
					Messege = ex.Message,
					Errors = new List<string> { "Something went wrong!! try again later" }
				};
			}
		}
		public async Task<UserManagerResponse> RegisterLocalHomeOwnerAsync(RegisterLocalHomeOwnerModel model, string role)
		{
			try
			{
				if (model == null) throw new ArgumentNullException(nameof(model));

				var existingUser = await _userManager.FindByEmailAsync(model.Email);
				if (existingUser != null)
				{
					return new UserManagerResponse
					{
						IsSuccess = false,
						Messege = "This Email is already associated with another Account",
						Errors = new List<string> { "This Email is already associated with another Account" }
					};
				}

				if (model.Password != model.ConfirmPassword)
				{
					return new UserManagerResponse
					{
						IsSuccess = false,
						Messege = "Confirm Password does not match the password",
						Errors = new List<string> { "Confirm Password does not match the password" }
					};
				}

				var user = new ApplicationUser
				{
					UserName = model.FullName,
					Email = model.Email,
					FullName = model.FullName,
					Role = role,
					PhoneNumber = model.PhoneNumber,
					Address = model.Address,
					Status = ProofStatus.Pending,
					IsDeleted = false
				};

				var result = await _userManager.CreateAsync(user, model.Password);
				if (!result.Succeeded)
				{
					return new UserManagerResponse
					{
						IsSuccess = false,
						Messege = "User registration failed",
						Errors = result.Errors.Select(e => e.Description)
					};
				}

				await _userManager.AddToRoleAsync(user, role);

				// Save profile photo
				if (model.ProfilePhoto != null)
				{
					var saveResult = await SaveFileWithValidation(user.Id, "profilephotos", model.ProfilePhoto);
					if (!saveResult.IsSuccess)
					{
						await _userManager.DeleteAsync(user);
						return new UserManagerResponse { IsSuccess = false, Messege = saveResult.Messege };
					}
					user.ProfilePhotoPath = saveResult.Path;
					await _userManager.UpdateAsync(user);
				}

				// Save Property Ownership document
				if (model.PropertyOwnerShipDoc != null)
				{
					var propertyRegSaveResult = await SaveFileWithValidation(user.Id, "proofs", model.PropertyOwnerShipDoc);
					if (!propertyRegSaveResult.IsSuccess)
					{
						await _userManager.DeleteAsync(user);
						return new UserManagerResponse { IsSuccess = false, Messege = propertyRegSaveResult.Messege };
					}

					_context.Proofs.Add(new Proof
					{
						UserId = user.Id,
						DocumentType = "Property OwnerShip Form",
						DocumentPath = propertyRegSaveResult.Path
					});
				}

				// Save Image Gallery documents
				if (model.ImageGalleryDoc != null && model.ImageGalleryDoc.Any())
				{
					foreach (var image in model.ImageGalleryDoc)
					{
						var imageSaveResult = await SaveFileWithValidation(user.Id, "proofs", image);
						if (!imageSaveResult.IsSuccess)
						{
							await _userManager.DeleteAsync(user);
							return new UserManagerResponse { IsSuccess = false, Messege = imageSaveResult.Messege };
						}

						_context.Proofs.Add(new Proof
						{
							UserId = user.Id,
							DocumentType = "Image Gallery",
							DocumentPath = imageSaveResult.Path
						});
					}
				}

				// Save the proofs in the database
				await _context.SaveChangesAsync();

				return new UserManagerResponse { IsSuccess = true, Messege = "User registered successfully." };
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while registering local home owner.");
				var user = await _userManager.FindByEmailAsync(model.Email);
				if (user != null)
				{
					await _userManager.DeleteAsync(user);
				}

				return new UserManagerResponse
				{
					IsSuccess = false,
					Messege = ex.Message,
					Errors = new List<string> { "Something went wrong!! try again later" }
				};
			}
		}
		public async Task<UserManagerResponse> RegisterHotelOwnerAsync(RegisterHotelOwnerModel model, string role)
		{
			try
			{
				if (model == null) throw new ArgumentNullException(nameof(model));

				var existingUser = await _userManager.FindByEmailAsync(model.Email);
				if (existingUser != null)
				{
					return new UserManagerResponse
					{
						IsSuccess = false,
						Messege = "This Email is already associated with another Account",
						Errors = new List<string> { "This Email is already associated with another Account" }
					};
				}

				if (model.Password != model.ConfirmPassword)
				{
					return new UserManagerResponse
					{
						IsSuccess = false,
						Messege = "Confirm Password does not match the password",
						Errors = new List<string> { "Confirm Password does not match the password" }
					};
				}

				var user = new ApplicationUser
				{
					UserName = model.FullName,
					Email = model.Email,
					FullName = model.FullName,
					Role = role,
					PhoneNumber = model.PhoneNumber,
					Address = model.Address,
					Status = ProofStatus.Pending,
					IsDeleted = false
				};

				var result = await _userManager.CreateAsync(user, model.Password);
				if (!result.Succeeded)
				{
					return new UserManagerResponse
					{
						IsSuccess = false,
						Messege = "User registration failed",
						Errors = result.Errors.Select(e => e.Description)
					};
				}

				await _userManager.AddToRoleAsync(user, role);

				// Save profile photo
				if (model.ProfilePhoto != null)
				{
					var saveResult = await SaveFileWithValidation(user.Id, "profilephotos", model.ProfilePhoto);
					if (!saveResult.IsSuccess)
					{
						await _userManager.DeleteAsync(user);
						return new UserManagerResponse { IsSuccess = false, Messege = saveResult.Messege };
					}
					user.ProfilePhotoPath = saveResult.Path;
					await _userManager.UpdateAsync(user);
				}

				// Save Property Ownership document
				if (model.PropertyOwnerShipDoc != null)
				{
					var propertyRegSaveResult = await SaveFileWithValidation(user.Id, "proofs", model.PropertyOwnerShipDoc);
					if (!propertyRegSaveResult.IsSuccess)
					{
						await _userManager.DeleteAsync(user);
						return new UserManagerResponse { IsSuccess = false, Messege = propertyRegSaveResult.Messege };
					}

					_context.Proofs.Add(new Proof
					{
						UserId = user.Id,
						DocumentType = "Property OwnerShip Form",
						DocumentPath = propertyRegSaveResult.Path
					});
				}

				// Save Image Gallery documents
				if (model.ImageGalleryDoc != null && model.ImageGalleryDoc.Any())
				{
					foreach (var image in model.ImageGalleryDoc)
					{
						var imageSaveResult = await SaveFileWithValidation(user.Id, "proofs", image);
						if (!imageSaveResult.IsSuccess)
						{
							await _userManager.DeleteAsync(user);
							return new UserManagerResponse { IsSuccess = false, Messege = imageSaveResult.Messege };
						}

						_context.Proofs.Add(new Proof
						{
							UserId = user.Id,
							DocumentType = "Image Gallery",
							DocumentPath = imageSaveResult.Path
						});
					}
				}

				// Save the proofs in the database
				await _context.SaveChangesAsync();

				return new UserManagerResponse { IsSuccess = true, Messege = "User registered successfully." };
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while registering local home owner.");
				var user = await _userManager.FindByEmailAsync(model.Email);
				if (user != null)
				{
					await _userManager.DeleteAsync(user);
				}

				return new UserManagerResponse
				{
					IsSuccess = false,
					Messege = ex.Message,
					Errors = new List<string> { "Something went wrong!! try again later" }
				};
			}
		}

		//public async Task<UserManagerResponse> RegisterTourGuideAsync(RegisterTourGuideModel model, string role)
		//{
		//	try
		//	{
		//		if (model == null) throw new ArgumentNullException(nameof(model));

		//		var existingUser = await _userManager.FindByEmailAsync(model.Email);
		//		if (existingUser != null)
		//		{
		//			return new UserManagerResponse
		//			{
		//				IsSuccess = false,
		//				Messege = "This Email is already associated with another Account"
		//			};
		//		}

		//		if (model.Password != model.ConfirmPassword)
		//		{
		//			return new UserManagerResponse
		//			{
		//				IsSuccess = false,
		//				Messege = "Confirm Password does not match the password"
		//			};
		//		}

		//		var user = new ApplicationUser
		//		{
		//			UserName = model.FullName,
		//			Email = model.Email,
		//			FullName = model.FullName,
		//			Role = role,
		//			PhoneNumber = model.PhoneNumber,
		//			Address = model.Address,
		//			Status = ProofStatus.Pending,
		//		};

		//		var result = await _userManager.CreateAsync(user, model.Password);
		//		if (!result.Succeeded)
		//		{
		//			return new UserManagerResponse
		//			{
		//				IsSuccess = false,
		//				Messege = "User registration failed",
		//				Errors = result.Errors.Select(e => e.Description)
		//			};
		//		}

		//		await _userManager.AddToRoleAsync(user, role);

		//		// Save profile photo
		//		if (model.ProfilePhoto != null)
		//		{
		//			var saveResult = await SaveFileWithValidation(user.Id, "profilephotos", model.ProfilePhoto);
		//			if (!saveResult.IsSuccess)
		//			{
		//				await _userManager.DeleteAsync(user);
		//				return new UserManagerResponse { IsSuccess = false, Messege = saveResult.Messege };
		//			}
		//			user.ProfilePhotoPath = saveResult.Path;
		//			await _userManager.UpdateAsync(user);
		//		}

		//		// Save Property Ownership document
		//		if (model.IdCard != null)
		//		{
		//			var IdCard = await SaveFileWithValidation(user.Id, "proofs", model.IdCard);
		//			if (!IdCard.IsSuccess)
		//			{
		//				await _userManager.DeleteAsync(user);
		//				return new UserManagerResponse { IsSuccess = false, Messege = IdCard.Messege };
		//			}

		//			_context.Proofs.Add(new Proof
		//			{
		//				UserId = user.Id,
		//				DocumentType = "Id Card",
		//				DocumentPath = IdCard.Path
		//			});
		//		}

		//		// Save the proofs in the database
		//		await _context.SaveChangesAsync();

		//		return new UserManagerResponse { IsSuccess = true, Messege = "User registered successfully." };
		//	}
		//	catch (Exception ex)
		//	{
		//		_logger.LogError(ex, "Error occurred while registering local home owner.");
		//		var user = await _userManager.FindByEmailAsync(model.Email);
		//		if (user != null)
		//		{
		//			await _userManager.DeleteAsync(user);
		//		}

		//		return new UserManagerResponse
		//		{
		//			IsSuccess = false,
		//			Messege = ex.Message
		//		};
		//	}
		//}

		public async Task<UserManagerResponse> RegisterTourGuideAsync(RegisterTourGuideModel model, string role)
		{
			try
			{
				if (model == null) throw new ArgumentNullException(nameof(model));

				// Check if email already exists
				var existingUser = await _userManager.FindByEmailAsync(model.Email);
				if (existingUser != null)
				{
					return new UserManagerResponse
					{
						IsSuccess = false,
						Messege = "This Email is already associated with another Account",
						Errors = new List<string> { "This Email is already associated with another Account" }
					};
				}

				// Confirm password validation
				if (model.Password != model.ConfirmPassword)
				{
					return new UserManagerResponse
					{
						IsSuccess = false,
						Messege = "Confirm Password does not match the password",
						Errors = new List<string> { "Confirm Password does not match the password" }
					};
				}

				// Create new ApplicationUser
				var user = new ApplicationUser
				{
					UserName = model.Email,  // Better to use email as username
					Email = model.Email,
					FullName = model.FullName,
					Role = role,
					PhoneNumber = model.PhoneNumber,
					Address = model.Address,
					Status = ProofStatus.Pending, // Admin needs to verify first
					IsEmailConfirmed = false,
					IsDeleted = false
				};

				var result = await _userManager.CreateAsync(user, model.Password);
				if (!result.Succeeded)
				{
					return new UserManagerResponse
					{
						IsSuccess = false,
						Messege = "User registration failed",
						Errors = result.Errors.Select(e => e.Description)
					};
				}

				// Assign role
				await _userManager.AddToRoleAsync(user, role);

				// Save Profile Photo
				if (model.ProfilePhoto != null)
				{
					var saveResult = await SaveFileWithValidation(user.Id, "profilephotos", model.ProfilePhoto);
					if (!saveResult.IsSuccess)
					{
						await _userManager.DeleteAsync(user);
						return new UserManagerResponse { IsSuccess = false, Messege = saveResult.Messege };
					}
					user.ProfilePhotoPath = saveResult.Path;
					await _userManager.UpdateAsync(user);
				}

				// Save ID Card Proof
				if (model.IdCard != null)
				{
					var idCard = await SaveFileWithValidation(user.Id, "proofs", model.IdCard);
					if (!idCard.IsSuccess)
					{
						await _userManager.DeleteAsync(user);
						return new UserManagerResponse { IsSuccess = false, Messege = idCard.Messege };
					}

					_context.Proofs.Add(new Proof
					{
						UserId = user.Id,
						DocumentType = "Id Card",
						DocumentPath = idCard.Path
					});
				}

				// Save new Tour Guide Entry in TourGuide Table
				var tourGuide = new TourGuide
				{
					UserId = user.Id,
					FullName = model.FullName,
					PhoneNumber = model.PhoneNumber,
					RatePerDay = model.RatePerDay,
					Experience = model.Experience,
					Bio = model.Bio,
					Location = model.Location,
					IsAvailable = false // Default false, Admin will enable it
				};

				_context.TourGuides.Add(tourGuide);

				// Save all changes
				await _context.SaveChangesAsync();

				return new UserManagerResponse { IsSuccess = true, Messege = "Tour Guide registered successfully. Awaiting admin approval." };
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while registering Tour Guide.");
				var user = await _userManager.FindByEmailAsync(model.Email);
				if (user != null)
				{
					await _userManager.DeleteAsync(user);
				}

				return new UserManagerResponse
				{
					IsSuccess = false,
					Messege = ex.Message,
					Errors = new List<string> { "Something went wrong!! try again later" }
				};
			}
		}

		public async Task<UserManagerResponse> RegisterTouristAsync(RegisterTouristModel model, string role)
		{
			try
			{
				if (model == null) throw new ArgumentNullException(nameof(model));

				var existingUser = await _userManager.FindByEmailAsync(model.Email);
				if (existingUser != null)
				{
					return new UserManagerResponse
					{
						IsSuccess = false,
						Messege = "This Email is already associated with another Account",
						Errors = new List<string> { "This Email is already associated with another Account" }
					};
				}

				if (model.Password != model.ConfirmPassword)
				{
					return new UserManagerResponse
					{
						IsSuccess = false,
						Messege = "Confirm Password does not match the password",
						Errors = new List<string> { "Confirm Password does not match the password" }
					};
				}

				var user = new ApplicationUser
				{
					UserName = model.FullName,
					Email = model.Email,
					FullName = model.FullName,
					Role = role,
					PhoneNumber = model.PhoneNumber,
					Address = model.Address,
					Status = ProofStatus.Verified,
					IsDeleted = false
				};

				var result = await _userManager.CreateAsync(user, model.Password);
				if (!result.Succeeded)
				{
					return new UserManagerResponse
					{
						IsSuccess = false,
						Messege = "User Name already Exist Try Changing UserName",
						Errors = result.Errors.Select(e => e.Description)
					};
				}

				await _userManager.AddToRoleAsync(user, role);

				if (model.ProfilePhoto != null)
				{
					var saveResult = await SaveFileWithValidation(user.Id, "profilephotos", model.ProfilePhoto);
					if (!saveResult.IsSuccess)
					{
						await _userManager.DeleteAsync(user);
						return new UserManagerResponse { IsSuccess = false, Messege = saveResult.Messege };
					}
					user.ProfilePhotoPath = saveResult.Path;
					await _userManager.UpdateAsync(user);
				}

				return new UserManagerResponse { IsSuccess = true, Messege = "User registered successfully." };
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while registering tourist.");
				return new UserManagerResponse
				{
					IsSuccess = false,
					Messege = ex.Message,
					Errors = new List<string> { "Something went wrong!! try again later" }
				};
			}
		}
		#endregion
		#region Email Confirmation
		public async Task<bool> SendEmailVerificationAsync(string email)
		{
			var user = await _userManager.FindByEmailAsync(email);
			if (user == null) return false;
			string token;
			var subject = "";
			string emailContent;
			if (!user.EmailConfirmed)
			{
				token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
				var userId = user.Id.ToString();
				var backendUrl =_config.GetValue<string>("Backend_Url");
				// Backend API endpoint for email confirmation
				var callbackUrl = $"{backendUrl}/api/auth/confirm-email?token={Uri.EscapeDataString(token)}&userId={Uri.EscapeDataString(userId)}";

				emailContent = EmailTemplates.EmailVerificationTemplate(user.FullName, callbackUrl);
				subject = "Confirm Your Email";
			}
			else
			{
				token = await _userManager.GeneratePasswordResetTokenAsync(user);
				// navigate to change password page
				var userId = user.Id.ToString();
				var frontendUrl = _config.GetValue<string>("Front_Url");
				// Construct the callback URL with token and userId as query parameters
				var callbackUrl = $"{frontendUrl}/change-password?token={Uri.EscapeDataString(token)}&userId={Uri.EscapeDataString(userId)}";

				emailContent = EmailTemplates.EmailVerificationTemplate(user.FullName, callbackUrl);
				subject = "Reset Your Password";
			}
			var result = await _emailService.SendEmailAsync(email, subject, emailContent);
			return result;
		}
		public async Task<UserManagerResponse> ConfirmEmail(string userId, string token)
		{
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
				return new UserManagerResponse
				{
					IsSuccess = false,
					Messege = "Invalid User"
				};
			if (user.EmailConfirmed) 
			{
				return new UserManagerResponse
				{
					IsSuccess = true,
					Messege = "Email Already Verified"
				};
			}

			var result = await _userManager.ConfirmEmailAsync(user, token);
			if (result.Succeeded)
			{
				user.IsEmailBounced = false;
				string link;
				var frontendUrl = _config.GetValue<string>("Front_Url");
				var roles = await _userManager.GetRolesAsync(user);
				if (roles.Contains("Tourist"))
				{
					link = $"{frontendUrl}/email-verified";  // Redirect to the email verified page for tourists
				}
				else
				{
					// If the user is not a tourist, redirect to the pending approval page
					 link = $"{frontendUrl}/account-pending-approval";  // This page informs the user that their info is sent for admin approval
				}
				return new UserManagerResponse
				{
					IsSuccess = true,
					Messege = link
				};
			}

			return new UserManagerResponse
			{
				IsSuccess = false,
				Messege = "Error confirming email."
			};
		}

		public async Task<UserManagerResponse> SendAccountApprovedEmail(string email)
		{
			var user = await _userManager.FindByEmailAsync(email);
			if (user == null)
			{
				return new UserManagerResponse
				{
					IsSuccess = false,
					Messege = "User Not Found"
				};
			}
			var frontendUrl = _config.GetValue<string>("Front_Url");
			var subject = "Account Approved";
			var loginLink = $"{frontendUrl}/login"; // Change this to your actual login page URL
			string emailContent = EmailTemplates.AccountApprovedTemplate(user.FullName, loginLink);

			// Sending email
			try
			{
				await _emailService.SendEmailAsync(user.Email, subject, emailContent);
				return new UserManagerResponse
				{
					IsSuccess = true,
					Messege = "Account approval email sent successfully."
				};
			}
			catch (Exception ex)
			{
				return new UserManagerResponse
				{
					IsSuccess = false,
					Messege = $"Failed to send email. Error: {ex.Message}"
				};
			}
		}

		public async Task<UserManagerResponse> SendAccountRejectedEmail(string email)
		{
			var frontendUrl = _config.GetValue<string>("Front_Url");
			var subject = "Account Rejected";
			var registerLink = $"{frontendUrl}/register"; // Change this to your actual register page URL
			string emailContent = EmailTemplates.AccountRejectedTemplate(email, registerLink);

			// Sending email
			try
			{
				await _emailService.SendEmailAsync(email, subject,emailContent);
				return new UserManagerResponse
				{
					IsSuccess = true,
					Messege = "Account rejection email sent successfully."
				};
			}
			catch (Exception ex)
			{
				return new UserManagerResponse
				{
					IsSuccess = false,
					Messege = $"Failed to send email. Error: {ex.Message}"
				};
			}
		}
		#endregion

		#region Login

		public async Task<UserManagerResponse> LoginUserAsync(LoginModel model)
		{
			var user = await _userManager.FindByEmailAsync(model.Email);
			if (user == null)
				return new UserManagerResponse { IsSuccess = false, Messege = "This Email is not associated with any Account.. Try Signing Up!!" };

			if (!await _userManager.IsEmailConfirmedAsync(user))
				return new UserManagerResponse { IsSuccess = false, Messege = "Email not confirmed." };

			if (user.Status == ProofStatus.Pending)
				return new UserManagerResponse { IsSuccess = false, Messege = "Your Info has been sent to Admin.. After the approval you will get an email.. Then You can login!" };

			if (!await _userManager.CheckPasswordAsync(user, model.Password))
				return new UserManagerResponse { IsSuccess = false, Messege = "Invalid password." };

			// Generate JWT token
			var token = _JwtHelper.GenerateToken(user.Id.ToString(), model.Email, user.Role);
			var jwt = new JwtSecurityTokenHandler().WriteToken(token);

			// Set token as HttpOnly cookie
			// Clear all old role-based cookies
			_httpContextAccessor.HttpContext?.Response.Cookies.Delete("authToken");
			// Set a universal token (optional: you can still customize per role if needed)
			_httpContextAccessor.HttpContext?.Response.Cookies.Append("authToken", jwt, new CookieOptions
			{
				HttpOnly = true,
				Secure = true, // Required for SameSite=None to work in Chrome
				SameSite = SameSiteMode.None,
				Expires = DateTime.Now.AddHours(1)
			});


			// Return user data (do NOT return token in body)
			return new UserManagerResponse
			{
				IsSuccess = true,
				Messege = "Login successful.",
				Data = new
				{
					userId = user.Id,
					role = user.Role,
					email = user.Email
				}
			};
		}



		#endregion

		#region Password Management

		//public async Task<bool> SendPasswordResetLinkAsync(string email)
		//{
		//	var user = await _userManager.FindByEmailAsync(email);
		//	if (user == null) return false;

		//	var token = await _userManager.GeneratePasswordResetTokenAsync(user);
		//	var callbackUrl = _urlHelper.Action(
		//		action: "ResetPassword",
		//		controller: "Auth",
		//		values: new { userId = user.Id, token },
		//		protocol: _httpContextAccessor.HttpContext.Request.Scheme
		//	);

		//	var emailContent = EmailTemplates.ForgetPasswordEmailTemplate(user.FullName, callbackUrl);
		//	var result = await _emailService.SendEmailAsync(email, "Reset Your Password", emailContent);

		//	return result;

		//}

		public async Task<UserManagerResponse> ResetPasswordAsync(ResetPasswordModel model)
		{
			try
			{
				var user = await _userManager.FindByIdAsync(model.UserId);
				if (user == null)
				{
					return new UserManagerResponse
					{
						IsSuccess = false,
						Messege = "No user found with this email."
					};
				}
				if(model.NewPassword != model.ConfirmPassword)
				{
					return new UserManagerResponse
					{
						IsSuccess = false,
						Messege = "Password and Confirm Password Mismatch!"
					};
				}

				var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
				if (result.Succeeded)
				{
					return new UserManagerResponse
					{
						IsSuccess = true,
						Messege = "Password reset successfully."
					};
				}

				return new UserManagerResponse
				{
					IsSuccess = false,
					Messege = "Failed to reset password.",
					Errors = result.Errors.Select(e => e.Description)
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error in ResetPasswordAsync");
				return new UserManagerResponse
				{
					IsSuccess = false,
					Messege = "An error occurred. Please try again later."
				};
			}
		}
		#endregion

		#region Utility Methods

		//private async Task SaveProof(Guid userId, string documentType, IFormFile file)
		//{
		//	var saveResult = await SaveFileWithValidation(userId, "proofs", file);
		//	if (saveResult.IsSuccess)
		//	{
		//		_context.Proofs.Add(new Proof
		//		{
		//			UserId = userId,
		//			DocumentType = documentType,
		//			DocumentPath = saveResult.Path
		//		});
		//		await _context.SaveChangesAsync();
		//	}
		//	else
		//	{
		//		throw new InvalidOperationException($"Error saving {documentType}: {saveResult.Messege}");
		//	}
		//}

		//private async Task<FileSaveResult> SaveFileWithValidation(Guid userId, string directory, IFormFile file)
		//{
		//	try
		//	{
		//		var allowedExtensions = directory == "proofs"
		//			? new[] { ".pdf", ".jpg", ".jpeg", ".png" }
		//			: new[] { ".jpg", ".jpeg", ".png" };
		//		var maxFileSize = 5 * 1024 * 1024;

		//		var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
		//		if (!allowedExtensions.Contains(extension))
		//			return new FileSaveResult { IsSuccess = false, Messege = "Invalid file type." };

		//		if (file.Length > maxFileSize)
		//			return new FileSaveResult { IsSuccess = false, Messege = "File size exceeds limit." };

		//		// Get the blob container client
		//		var containerName = directory == "proofs" ? _proofContainerName : _profilePhotoContainerName;
		//		var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

		//		// Create the container if it doesn't exist
		//		await containerClient.CreateIfNotExistsAsync();

		//		// Generate a unique blob name
		//		var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
		//		var blobClient = containerClient.GetBlobClient(fileName);

		//		// Upload the file to Blob Storage
		//		using (var stream = file.OpenReadStream())
		//		{
		//			await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });
		//		}

		//		// Return the blob URL
		//		var blobUrl = blobClient.Uri.ToString();
		//		return new FileSaveResult { IsSuccess = true, Path = blobUrl };
		//	}
		//	catch (Exception ex)
		//	{
		//		_logger.LogError(ex, "Error saving file to Blob Storage.");
		//		return new FileSaveResult { IsSuccess = false, Messege = "An error occurred while saving the file to Blob Storage." };
		//	}
		//}
		private async Task SaveProof(Guid userId, string documentType, IFormFile file)
		{
			var saveResult = await SaveFileWithValidation(userId, "proofs", file);
			if (saveResult.IsSuccess)
			{
				_context.Proofs.Add(new Proof
				{
					UserId = userId,
					DocumentType = documentType,
					DocumentPath = saveResult.Path
				});
				await _context.SaveChangesAsync();
			}
			else
			{
				throw new InvalidOperationException($"Error saving {documentType}: {saveResult.Messege}");
			}
		}

		//private async Task<FileSaveResult> SaveFileWithValidation(Guid userId, string directory, IFormFile file)
		//{
		//	try
		//	{
		//		var allowedExtensions = directory == "proofs"
		//			? new[] { ".pdf", ".jpg", ".jpeg", ".png" }
		//			: new[] { ".jpg", ".jpeg", ".png" };
		//		var maxFileSize = 5 * 1024 * 1024; // 5 MB

		//		var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
		//		if (!allowedExtensions.Contains(extension))
		//			return new FileSaveResult { IsSuccess = false, Messege = "Invalid file type." };

		//		if (file.Length > maxFileSize)
		//			return new FileSaveResult { IsSuccess = false, Messege = "File size exceeds limit." };

		//		// Define the storage path based on directory type
		//		var rootPath = directory == "proofs"
		//			? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "proofs", userId.ToString())
		//			: Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", directory);

		//		// Ensure the directory exists
		//		if (!Directory.Exists(rootPath))
		//		{
		//			Directory.CreateDirectory(rootPath);
		//		}

		//		// Generate a unique file name
		//		var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
		//		var filePath = Path.Combine(rootPath, fileName);

		//		// Save the file locally
		//		using (var stream = new FileStream(filePath, FileMode.Create))
		//		{
		//			await file.CopyToAsync(stream);
		//		}

		//		// Return the relative path
		//		var relativePath = Path.Combine(directory == "proofs" ? $"proofs/{userId}" : directory, fileName).Replace("\\", "/");
		//		return new FileSaveResult { IsSuccess = true, Path = $"/{relativePath}" };
		//	}
		//	catch (Exception ex)
		//	{
		//		_logger.LogError(ex, "Error saving file to local storage.");
		//		return new FileSaveResult { IsSuccess = false, Messege = "An error occurred while saving the file to local storage." };
		//	}
		//}

		private async Task<FileSaveResult> SaveFileWithValidation(Guid userId, string directory, IFormFile file)
		{
			try
			{
				var allowedExtensions = directory == "proofs"
					? new[] { ".pdf", ".jpg", ".jpeg", ".png" }
					: new[] { ".jpg", ".jpeg", ".png" };
				var maxFileSize = 5 * 1024 * 1024; // 5 MB

				var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
				if (!allowedExtensions.Contains(extension))
					return new FileSaveResult { IsSuccess = false, Messege = "Invalid file type." };

				if (file.Length > maxFileSize)
					return new FileSaveResult { IsSuccess = false, Messege = "File size exceeds limit." };

				// Define the storage path based on directory type
				var rootPath = directory == "proofs"
					? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "proofs", userId.ToString())
					: Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", directory);

				// Ensure the directory exists
				if (!Directory.Exists(rootPath))
				{
					Directory.CreateDirectory(rootPath);
				}

				// Generate a unique file name
				var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
				var filePath = Path.Combine(rootPath, fileName);

				// Save the file locally
				using (var stream = new FileStream(filePath, FileMode.Create))
				{
					await file.CopyToAsync(stream);
				}

				// Return the correct relative path (excluding wwwroot)
				var relativePath = Path.Combine(directory == "proofs" ? $"proofs/{userId}" : directory, fileName)
					.Replace("\\", "/");

				return new FileSaveResult { IsSuccess = true, Path = $"/{relativePath}" };
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error saving file to local storage.");
				return new FileSaveResult { IsSuccess = false, Messege = "An error occurred while saving the file to local storage." };
			}
		}


		#endregion
		#region User Management

		public async Task<UserManagerResponse> UpdateUser(UpdateUserModel model,string userId)
		{
			try
			{
				var user = await _userManager.FindByIdAsync(userId);
				if (user == null)
				{
					return new UserManagerResponse
					{
						IsSuccess = false,
						Messege = "User Not Found"
					};
				}
				user.FullName = model.FullName;
				user.Address = model.Address;
				if (model.ProfilePhoto != null)
				{
					var filePath = await SaveFileWithValidation(user.Id, "profilephotos", model.ProfilePhoto);
					if (!filePath.IsSuccess)
					{
						return new UserManagerResponse
						{
							IsSuccess = false,
							Messege = "Something went wrong while saving the file"
						};
					}
					user.ProfilePhotoPath = filePath.Path;
				}
				var result = await _userManager.UpdateAsync(user);
				if (result.Succeeded)
				{
					return new UserManagerResponse
					{
						IsSuccess = true,
						Messege = "User data updated succesfully"
					};
				}
				return new UserManagerResponse
				{
					IsSuccess = false,
					Messege = "Something went wrong"
				};
			}
			catch (Exception ex)
			{
				_logger.LogInformation("Failed to update user ",ex.Message);
				return new UserManagerResponse
				{
					IsSuccess = false,
					Messege = ex.Message
				};
			}
		}
		public async Task<bool> IsAdmin(string email)
		{
			var user = await _userManager.FindByEmailAsync(email);
			if(user == null) return false;
			var result = await _userManager.IsInRoleAsync(user, "Admin");
			return result;
		}
		public async Task<UserManagerResponse> DeleteUser(string userEmail)
		{
			var user = await _userManager.FindByEmailAsync(userEmail);
			if (user == null)
				return new UserManagerResponse { IsSuccess = true, Messege = "User not found." };

			var result = await _userManager.DeleteAsync(user);
			return result.Succeeded
				? new UserManagerResponse { IsSuccess = true, Messege = "User deleted successfully." }
				: new UserManagerResponse { IsSuccess = false, Messege = "Error deleting user." };
		}

		//public async Task<UserManagerResponse> GetUsersAllBookings(string userId)
		//{
		//		var result = new List<UserBookingSummaryDto>();

		//		// 🏨 Hotel Room Bookings
		//		var hotelBookings = await _context.UserHotelRoomBookings
		//			.Include(b => b.HotelRoom).ThenInclude(r => r.Hotel)
		//			.Where(b => b.UserId.ToString() == userId)
		//			.ToListAsync();

		//		result.AddRange(hotelBookings.Select(b => new UserBookingSummaryDto
		//		{
		//			BookingId =b.BookingId,
		//			BookingType = "Hotel",
		//			ServiceName = b.HotelRoom.Hotel.HotelName,
		//			Location = null,
		//			BookingDate = b.BookingTime,
		//			AmountPaid = b.TotalAmount,
		//			NumberOfPeople = null,
		//			Status = b.Status
		//		}));

		//		// 🏡 Local Home Bookings
		//		var localHomeBookings = await _context.UserLocalHomesBookings
		//			.Include(b => b.LocalHome)
		//			.Where(b => b.UserId.ToString() == userId)
		//			.ToListAsync();

		//		result.AddRange(localHomeBookings.Select(b => new UserBookingSummaryDto
		//		{
		//			BookingId = b.Id,
		//			BookingType = "LocalHome",
		//			ServiceName = b.LocalHome.Name,
		//			Location = null,
		//			BookingDate = b.BookingDate,
		//			NumberOfPeople = null,
		//			AmountPaid = b.TotalAmount,
		//			Status = b.Status
		//		}));

		//		// 🚍 Transport Bookings
		//		var transportBookings = await _context.UserTransportBookings
		//			.Include(b => b.Transport)
		//			.Where(b => b.UserId.ToString() == userId)
		//			.ToListAsync();

		//		result.AddRange(transportBookings.Select(b => new UserBookingSummaryDto
		//		{
		//			BookingId= b.Id,
		//			BookingType = "Transport",
		//			ServiceName = b.Transport.Name,
		//			Location = $"{b.Transport.StartLocation} → {b.Transport.Destination}",
		//			BookingDate = b.BookingDate,
		//			NumberOfPeople = b.SeatsBooked,
		//			AmountPaid = b.TotalFare,
		//			Status = b.Status
		//		}));

		//		// 🧭 Tour Guide Bookings
		//		var tourGuideBookings = await _context.UserTourGuideBookings
		//			.Include(b => b.TourGuide)
		//			.Where(b => b.UserId.ToString() == userId)
		//			.ToListAsync();

		//		result.AddRange(tourGuideBookings.Select(b => new UserBookingSummaryDto
		//		{
		//			BookingId= b.Id,
		//			BookingType = "TourGuide",
		//			ServiceName = b.TourGuide.FullName,
		//			Location = null,
		//			BookingDate = b.BookingDate,
		//			AmountPaid = b.TotalAmount,
		//			NumberOfPeople = null,
		//			Status = b.Status
		//		}));

		//		// Sort by BookingDate (most recent first)
		//		return new UserManagerResponse
		//		{
		//			IsSuccess = true,
		//			Messege = "Bookings Retrieved Successfully",
		//			Data = result.OrderByDescending(b => b.BookingDate).ToList()
		//		};

		//}

		public async Task<UserManagerResponse> GetUsersAllBookings(string userId)
		{
			var result = new List<UserBookingSummaryDto>();

			// 🏨 Hotel Room Bookings
			var hotelBookings = await _context.UserHotelRoomBookings
				.Include(b => b.HotelRoom).ThenInclude(r => r.Hotel)
				.Where(b => b.UserId.ToString() == userId)
				.ToListAsync();

			result.AddRange(hotelBookings.Select(b => new UserBookingSummaryDto
			{
				BookingId = b.BookingId,
				BookingType = "Hotel",
				ServiceName = b.HotelRoom.Hotel.HotelName,
				Location = null,
				BookingDate = b.BookingTime,
				AmountPaid = b.TotalAmount,
				NumberOfPeople = null,
				Status = b.Status,
				StartDate = b.CheckInDate,
				EndDate = b.CheckOutDate
			}));

			// 🏡 Local Home Bookings
			var localHomeBookings = await _context.UserLocalHomesBookings
				.Include(b => b.LocalHome)
				.Where(b => b.UserId.ToString() == userId)
				.ToListAsync();

			result.AddRange(localHomeBookings.Select(b => new UserBookingSummaryDto
			{
				BookingId = b.Id,
				BookingType = "LocalHome",
				ServiceName = b.LocalHome.Name,
				Location = null,
				BookingDate = b.BookingDate,
				AmountPaid = b.TotalAmount,
				NumberOfPeople = null,
				Status = b.Status,
				StartDate = b.StartDate,
				EndDate = b.EndDate
			}));

			// 🚍 Transport Bookings
			var transportBookings = await _context.UserTransportBookings
				.Include(b => b.Transport)
				.Where(b => b.UserId.ToString() == userId)
				.ToListAsync();

			result.AddRange(transportBookings.Select(b => new UserBookingSummaryDto
			{
				BookingId = b.Id,
				BookingType = "Transport",
				ServiceName = b.Transport.Name,
				Location = $"{b.Transport.StartLocation} → {b.Transport.Destination}",
				BookingDate = b.BookingDate,
				AmountPaid = b.TotalFare,
				NumberOfPeople = b.SeatsBooked,
				Status = b.Status,
				StartDate = null,
				EndDate = null
			}));

			// 🧭 Tour Guide Bookings
			var tourGuideBookings = await _context.UserTourGuideBookings
				.Include(b => b.TourGuide)
				.Where(b => b.UserId.ToString() == userId)
				.ToListAsync();

			result.AddRange(tourGuideBookings.Select(b => new UserBookingSummaryDto
			{
				BookingId = b.Id,
				BookingType = "TourGuide",
				ServiceName = b.TourGuide.FullName,
				Location = null,
				BookingDate = b.BookingDate,
				AmountPaid = b.TotalAmount,
				NumberOfPeople = null,
				Status = b.Status,
				StartDate = DateOnly.FromDateTime(b.StartDate),
				EndDate = DateOnly.FromDateTime(b.EndDate)
			}));

			// Sort by BookingDate (most recent first)
			return new UserManagerResponse
			{
				IsSuccess = true,
				Messege = "Bookings Retrieved Successfully",
				Data = result.OrderByDescending(b => b.BookingDate).ToList()
			};
		}

		public async Task<UserManagerResponse> GetUserBookingDetails(string bookingType, string bookingId)
		{
			switch (bookingType)
			{
				case "Hotel":
					var hotelBooking = await _context.UserHotelRoomBookings
						.Include(b => b.HotelRoom).ThenInclude(r => r.Hotel)
						.FirstOrDefaultAsync(b => b.BookingId.ToString() == bookingId);

					if (hotelBooking == null) return new UserManagerResponse { IsSuccess = false,Messege = "Booking Not Found",Errors = new List<string> { "Booking Not Found" } };

					return new UserManagerResponse
					{
						IsSuccess = true,
						Messege ="Data Retrieved Successfull",
						Data = (new
						{
							BookingType = "Hotel",
							HotelName = hotelBooking.HotelRoom.Hotel.HotelName,
							RoomType = hotelBooking.HotelRoom.RoomType.ToString(),
							Address = hotelBooking.HotelRoom.Hotel.Address,
							CheckIn = hotelBooking.CheckInDate,
							CheckOut = hotelBooking.CheckOutDate,
							TotalAmount = hotelBooking.TotalAmount,
							Status = hotelBooking.Status
						})
					};

				case "LocalHome":
					var homeBooking = await _context.UserLocalHomesBookings
						.Include(b => b.LocalHome)
						.FirstOrDefaultAsync(b => b.Id.ToString() == bookingId);

					if (homeBooking == null) return new UserManagerResponse { IsSuccess = false, Messege = "Booking Not Found", Errors = new List<string> { "Booking Not Found" } };

					return new UserManagerResponse
					{
						IsSuccess = true,
						Messege = "Data Retrieved Successfull",
						Data = (new
						{
							BookingType = "LocalHome",
							Name = homeBooking.LocalHome.Name,
							Address = homeBooking.LocalHome.AddressLine,
							BookingDate = homeBooking.BookingDate,
							TotalAmount = homeBooking.TotalAmount,
							Status = homeBooking.Status
						})
					};

				case "Transport":
					var transportBooking = await _context.UserTransportBookings
						.Include(b => b.Transport)
						.FirstOrDefaultAsync(b => b.Id.ToString() == bookingId);

					if (transportBooking == null) return new UserManagerResponse { IsSuccess = false, Messege = "Booking Not Found", Errors = new List<string> { "Booking Not Found" } };


					return new UserManagerResponse
					{
						IsSuccess = true,
						Messege = "Data Retrieved Successfull",
						Data = (new
						{
							BookingType = "Transport",
							TransportName = transportBooking.Transport.Name,
							Route = $"{transportBooking.Transport.StartLocation} → {transportBooking.Transport.Destination}",
							SeatsBooked = transportBooking.SeatsBooked,
							Date = transportBooking.BookingDate,
							TotalFare = transportBooking.TotalFare,
							Status = transportBooking.Status
						})
					};
				case "TourGuide":
					var guideBooking = await _context.UserTourGuideBookings
						.Include(b => b.TourGuide)
						.FirstOrDefaultAsync(b => b.Id.ToString() == bookingId);

					if (guideBooking == null) return new UserManagerResponse { IsSuccess = false, Messege = "Booking Not Found", Errors = new List<string> { "Booking Not Found" } };

						return new UserManagerResponse
						{
							IsSuccess = true,
							Messege = "Data Retrieved Successfull",
							Data = (new
							{
								BookingType = "TourGuide",
								GuideName = guideBooking.TourGuide.FullName,
								Location = guideBooking.TourGuide.Location,
								BookingDate = guideBooking.BookingDate,
								TotalAmount = guideBooking.TotalAmount,
								Status = guideBooking.Status
							})
						};

				default:
					return new UserManagerResponse
					{
						IsSuccess = false,
						Messege = "Invalid booking type."
					};
			}
		}



		public async Task<UserManagerResponse> GetUserInfo(string userId)
		{
			try
			{
				var user = await _userManager.FindByIdAsync(userId);

				if (user == null)
					return new UserManagerResponse
					{
						IsSuccess = false,
						Messege = "User Not Found"
					};

				return new UserManagerResponse
				{
					IsSuccess = true,
					Messege = "User retrived Successfully",
					Data = new
					{
						user.Email,
						user.UserName,
						user.Address,
						user.ProfilePhotoPath,
						user.Role
					}
				};
			}
			catch (Exception ex)
			{
				// Log the exception (if applicable)
				return new UserManagerResponse
				{
					IsSuccess = false,
					Messege = ex.Message
				};
			}
		}

		public async Task<object> GetTourGuideDetails(string userId)
		{
			var tourGuide = await _context.TourGuides.FirstOrDefaultAsync(tg => tg.UserId.ToString() == userId);
			if (tourGuide == null) return null;

			return new
			{
				tourGuide.RatePerDay,
				tourGuide.Experience,
				tourGuide.Bio,
				tourGuide.Location,
			};
		}
		[HttpGet("email-status")]
		public async Task<ApplicationUser?> GetEmailStatus(string email)
		{
			var user = await _userManager.FindByEmailAsync(email);
			if (user == null) return null;

			return user;
		}


		public async Task<UserManagerResponse> GetUserDetails(string userId)
		{
				// Get basic user info
				var userInfoResponse = await GetUserInfo(userId);
				if (!userInfoResponse.IsSuccess)
				{
					return userInfoResponse; // Return if user not found or an error occurred
				}

				var userData = userInfoResponse.Data as dynamic;
				var userRole = userData.Role; // Convert role to string if necessary

				object roleSpecificData = null;
				List<Proof> proofs = new List<Proof>();

				if (userRole == "TourGuide")
				{
					// Fetch additional details for TourGuide
					roleSpecificData = await GetTourGuideDetails(userId);
				}

				proofs = await _context.Proofs.Where(p => p.UserId.ToString() == userId).ToListAsync();

				return new UserManagerResponse
				{
					IsSuccess = true,
					Messege = "User details retrieved successfully",
					Data = new
					{
						User = userData,
						RoleSpecificDetails = roleSpecificData,
						Proofs = proofs.Select(p => new
						{
							p.DocumentType,  // Adjust fields according to your Proofs table
							p.DocumentPath
						})
					}
				};
			}
		}


		//public async Task<UserManagerResponse> GetUser()
		//{
		//	try
		//	{
		//		var user = await _userManager.FindByIdAsync();

		//		if (user == null)
		//			return new UserManagerResponse
		//			{
		//				IsSuccess = false,
		//				Messege = "User Not Found"
		//			};

		//		return new UserManagerResponse
		//		{
		//			IsSuccess = true,
		//			Messege = "User retrived Successfully",
		//			Data = new
		//			{
		//				Email = user.Email,
		//				UserName = user.UserName,
		//				Address = user.Address,
		//				ProfilePhotoUrl = user.ProfilePhotoPath
		//			}
		//		};
		//	}
		//	catch (Exception ex)
		//	{
		//		// Log the exception (if applicable)
		//		return new UserManagerResponse
		//		{
		//			IsSuccess = false,
		//			Messege = ex.Message
		//		};
		//	}
		//}
		#endregion

	}

	public class FileSaveResult
	{
		public bool IsSuccess { get; set; }
		public string Messege { get; set; }
		public string Path { get; set; }
	}
