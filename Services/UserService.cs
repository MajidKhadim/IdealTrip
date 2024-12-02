using IdealTrip.Helpers;
using IdealTrip.Models;
using IdealTrip.Models.Enums;
using IdealTrip.Models.Login;
using IdealTrip.Models.Register;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

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
		Task<bool> SendOtpAsync(string email);
		Task<bool> VerifyOtp(string email, string otp);
		Task<UserManagerResponse> DeleteUser(string userEmail);

		Task<UserManagerResponse> ForgotPasswordAsync(string email);
		Task<UserManagerResponse> ResetPasswordAsync(string email, string token, string newPassword);

		Task<bool> IsAdmin(string email);
	}

	public class UserService : IUserService
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly ApplicationDbContext _context;
		private readonly EmailService _emailService;
		private readonly IConfiguration _config;
		private readonly ILogger<UserService> _logger;
		private static readonly ConcurrentDictionary<string, (string Otp, DateTime Expiry)> _otpStorage = new();
		public readonly JwtHelper _JwtHelper;

		public UserService(
			UserManager<ApplicationUser> userManager,
			ApplicationDbContext context,
			EmailService emailService,
			IConfiguration config,
			ILogger<UserService> logger,
			JwtHelper jwtHelper)
		{
			_userManager = userManager;
			_context = context;
			_emailService = emailService;
			_config = config;
			_logger = logger;
			_JwtHelper = jwtHelper;
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
				if (model.VehicleRegistrationForm != null)
					await SaveProof(user.Id, "Vehicle Registration", model.VehicleRegistrationForm);

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
						Messege = "User registration failed",
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

		#region OTP Management

		public async Task<bool> SendOtpAsync(string email)
		{
			var otp = new Random().Next(100000, 999999).ToString();
			_otpStorage[email] = (otp, DateTime.UtcNow.AddMinutes(5));

			string emailContent = EmailTemplates.OtpEmailTemplate(otp);
			return await _emailService.SendEmailAsync(email, "Your OTP Code", emailContent);
		}

		public async Task<bool> VerifyOtp(string email, string otp)
		{
			if (_otpStorage.TryGetValue(email, out var otpInfo))
			{
				if (otpInfo.Otp == otp && DateTime.UtcNow <= otpInfo.Expiry)
				{
					_otpStorage.TryRemove(email, out _);
					var user = await _userManager.FindByEmailAsync(email);
					if (user != null) 
					{
						user.IsEmailConfirmed = true;
						user.EmailConfirmed = true;
						var result = await _userManager.UpdateAsync(user);

						return result.Succeeded;
					}
				}
			}
			return false;
		}

		#endregion

		#region Login

		public async Task<UserManagerResponse> LoginUserAsync(LoginModel model)
		{
			var user = await _userManager.FindByEmailAsync(model.Email);
			if (user == null)
				return new UserManagerResponse { IsSuccess = false, Messege = "User not found." };

			if (!await _userManager.IsEmailConfirmedAsync(user))
				return new UserManagerResponse { IsSuccess = false, Messege = "Email not confirmed." };

			if (!await _userManager.CheckPasswordAsync(user, model.Password))
				return new UserManagerResponse { IsSuccess = false, Messege = "Invalid password." };


			var token = _JwtHelper.GenerateToken(user.Id.ToString(), user.Email, user.Role);

			return new UserManagerResponse
			{
				IsSuccess = true,
				Messege = new JwtSecurityTokenHandler().WriteToken(token),
				Expiry = token.ValidTo
			};
		}


		#endregion

		#region Password Management

		public async Task<UserManagerResponse> ForgotPasswordAsync(string email)
		{
			try
			{
				var user = await _userManager.FindByEmailAsync(email);
				if (user == null)
				{
					return new UserManagerResponse
					{
						IsSuccess = false,
						Messege = "No user found with this email."
					};
				}

				var token = await _userManager.GeneratePasswordResetTokenAsync(user);
				var resetLink = $"{_config["BASE_URL"]}/reset-password?email={email}&token={Uri.EscapeDataString(token)}";

				string subject = "Reset Your Password";
				var emailContent = EmailTemplates.ForgetPasswordEmailTemplate(user.UserName,resetLink);
				var emailSent = await _emailService.SendEmailAsync(email, subject, emailContent);
				if (!emailSent)
				{
					return new UserManagerResponse
					{
						IsSuccess = false,
						Messege = "Failed to send password reset email."
					};
				}

				return new UserManagerResponse
				{
					IsSuccess = true,
					Messege = "Password reset email sent successfully."
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error in ForgotPasswordAsync");
				return new UserManagerResponse
				{
					IsSuccess = false,
					Messege = "An error occurred. Please try again later."
				};
			}
		}

		public async Task<UserManagerResponse> ResetPasswordAsync(string email, string token, string newPassword)
		{
			try
			{
				var user = await _userManager.FindByEmailAsync(email);
				if (user == null)
				{
					return new UserManagerResponse
					{
						IsSuccess = false,
						Messege = "No user found with this email."
					};
				}

				var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
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

				var userDir = Path.Combine("wwwroot", directory, userId.ToString());
				Directory.CreateDirectory(userDir);

				var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
				var fullPath = Path.Combine(userDir, fileName);

				using (var stream = new FileStream(fullPath, FileMode.Create))
				{
					await file.CopyToAsync(stream);
				}

				return new FileSaveResult { IsSuccess = true, Path = Path.Combine(directory, userId.ToString(), fileName).Replace("\\", "/") };
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error saving file for directory: {Directory}", directory);
				return new FileSaveResult { IsSuccess = false, Messege = "An error occurred while saving the file." };
			}
		}
		#endregion
		#region User Management

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

		#endregion
	}

	public class FileSaveResult
	{
		public bool IsSuccess { get; set; }
		public string Messege { get; set; }
		public string Path { get; set; }
	}
}
