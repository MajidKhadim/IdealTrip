using Azure.Core;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
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

		Task<UserManagerResponse> ConfirmEmail(string userId,string token);
		public Task<bool> SendEmailVerificationAsync(string email);
		Task<UserManagerResponse> SendAccountApprovedEmail(string email);
		Task<UserManagerResponse> SendAccountRejectedEmail(string email);
	}

	public class UserService : IUserService
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly ApplicationDbContext _context;
		private readonly EmailService _emailService;
		private readonly IConfiguration _config;
		private readonly ILogger<UserService> _logger;
		public readonly JwtHelper _JwtHelper;
		private readonly BlobServiceClient _blobServiceClient;
		private readonly string _profilePhotoContainerName = "profilepictures";
		private readonly string _proofContainerName = "proofs";

		public UserService(
			UserManager<ApplicationUser> userManager,
			ApplicationDbContext context,
			EmailService emailService,
			IConfiguration config,
			ILogger<UserService> logger,
			JwtHelper jwtHelper,
			BlobServiceClient blobServiceClient)
		{
			_userManager = userManager;
			_context = context;
			_emailService = emailService;
			_config = config;
			_logger = logger;
			_JwtHelper = jwtHelper;
			_blobServiceClient = blobServiceClient;
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
						Messege = "This Email is already associated with another Account"
					};
				}

				if (model.Password != model.ConfirmPassword)
				{
					return new UserManagerResponse
					{
						IsSuccess = false,
						Messege = "Confirm Password does not match the password"
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
					Messege = ex.Message
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
						Messege = "This Email is already associated with another Account"
					};
				}

				if (model.Password != model.ConfirmPassword)
				{
					return new UserManagerResponse
					{
						IsSuccess = false,
						Messege = "Confirm Password does not match the password"
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
					Messege = ex.Message
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
						Messege = "This Email is already associated with another Account"
					};
				}

				if (model.Password != model.ConfirmPassword)
				{
					return new UserManagerResponse
					{
						IsSuccess = false,
						Messege = "Confirm Password does not match the password"
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
					Messege = ex.Message
				};
			}
		}

		public async Task<UserManagerResponse> RegisterTourGuideAsync(RegisterTourGuideModel model, string role)
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
						Messege = "This Email is already associated with another Account"
					};
				}

				if (model.Password != model.ConfirmPassword)
				{
					return new UserManagerResponse
					{
						IsSuccess = false,
						Messege = "Confirm Password does not match the password"
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
				if (model.IdCard != null)
				{
					var IdCard = await SaveFileWithValidation(user.Id, "proofs", model.IdCard);
					if (!IdCard.IsSuccess)
					{
						await _userManager.DeleteAsync(user);
						return new UserManagerResponse { IsSuccess = false, Messege = IdCard.Messege };
					}

					_context.Proofs.Add(new Proof
					{
						UserId = user.Id,
						DocumentType = "Id Card",
						DocumentPath = IdCard.Path
					});
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
					Messege = ex.Message
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
					return new UserManagerResponse { IsSuccess = false, Messege = "This Email is already associated with another Account" };
				}

				if (model.Password != model.ConfirmPassword)
				{
					return new UserManagerResponse { IsSuccess = false, Messege = "Confirm Password does not match the password" };
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
					Messege = ex.Message
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
			var subject = "Account Rejected";
			var registerLink = $"{frontendUrl}/register"; // Change this to your actual register page URL
			string emailContent = EmailTemplates.AccountRejectedTemplate(user.FullName, registerLink);

			// Sending email
			try
			{
				await _emailService.SendEmailAsync(user.Email, subject, emailContent);
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


			var token = _JwtHelper.GenerateToken(user.Id.ToString(), model.Email, user.Role);

			return new UserManagerResponse
			{
				IsSuccess = true,
				Messege = new JwtSecurityTokenHandler().WriteToken(token),
				Expiry = token.ValidTo,
				Data = new
				{
					userId = user.Id,
					email = user.Email,
					userName = user.UserName,
					role = user.Role
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

		private async Task<FileSaveResult> SaveFileWithValidation(Guid userId, string directory, IFormFile file)
		{
			try
			{
				var allowedExtensions = directory == "proofs"
					? new[] { ".pdf", ".jpg", ".jpeg", ".png" }
					: new[] { ".jpg", ".jpeg", ".png" };
				var maxFileSize = 5 * 1024 * 1024;

				var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
				if (!allowedExtensions.Contains(extension))
					return new FileSaveResult { IsSuccess = false, Messege = "Invalid file type." };

				if (file.Length > maxFileSize)
					return new FileSaveResult { IsSuccess = false, Messege = "File size exceeds limit." };

				// Get the blob container client
				var containerName = directory == "proofs" ? _proofContainerName : _profilePhotoContainerName;
				var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

				// Create the container if it doesn't exist
				await containerClient.CreateIfNotExistsAsync();

				// Generate a unique blob name
				var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
				var blobClient = containerClient.GetBlobClient(fileName);

				// Upload the file to Blob Storage
				using (var stream = file.OpenReadStream())
				{
					await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });
				}

				// Return the blob URL
				var blobUrl = blobClient.Uri.ToString();
				return new FileSaveResult { IsSuccess = true, Path = blobUrl };
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error saving file to Blob Storage.");
				return new FileSaveResult { IsSuccess = false, Messege = "An error occurred while saving the file to Blob Storage." };
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
					Messege = "Something went wr"
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
				return new UserManagerResponse { IsSuccess = false, Messege = "User not found." };

			var result = await _userManager.DeleteAsync(user);
			return result.Succeeded
				? new UserManagerResponse { IsSuccess = true, Messege = "User deleted successfully." }
				: new UserManagerResponse { IsSuccess = false, Messege = "Error deleting user." };
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
						Email = user.Email,
						UserName = user.UserName,
						Address = user.Address,
						ProfilePhotoUrl = user.ProfilePhotoPath
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
		#endregion

	}

	public class FileSaveResult
	{
		public bool IsSuccess { get; set; }
		public string Messege { get; set; }
		public string Path { get; set; }
	}
}
