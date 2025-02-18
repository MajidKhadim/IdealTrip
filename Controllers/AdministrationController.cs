using IdealTrip.Models;
using IdealTrip.Models.AdminView;
using IdealTrip.Models.Enums;
using IdealTrip.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IdealTrip.Controllers
{
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	[Authorize(Roles ="Admin")]
	[Route("api/[controller]")]
	public class AdministrationController : Controller
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly ApplicationDbContext _context;
		private readonly IUserService _userService;

		public AdministrationController(UserManager<ApplicationUser> userManager, ApplicationDbContext context,IUserService userService)
		{
			_userManager = userManager;
			_context = context;
			_userService = userService;
		}

		[HttpGet("pending-users")]
		public async Task<ActionResult<DataSendingResponse>> GetPendingProofsUsers()
		{
			var pendingUsers = await _userManager.Users
				.Where(user => user.Status == ProofStatus.Pending)
				.ToListAsync();

			if (!pendingUsers.Any())
			{
				return Ok(new DataSendingResponse
				{
					IsSuccess = true,
					Message = "No pending users found.",
					Data = {}
				});
			}

			var userIds = pendingUsers.Select(u => u.Id).ToList();

			var proofs = await _context.Proofs
				.Where(proof => userIds.Contains(proof.UserId))
				.ToListAsync();

			var result = pendingUsers.Select(user => new PendingUsersAdminViewDto
			{
				UserId = user.Id,
				FullName = user.FullName,
				Email = user.Email,
				Role = user.Role,
				Status = user.Status,
				Proofs = proofs
					.Where(proof => proof.UserId == user.Id)
					.Select(proof => new ProofDto
					{
						DocumentType = proof.DocumentType,
						DocumentPath = proof.DocumentPath,
						UploadedAt = proof.UploadedAt
					})
					.ToList()
			}).ToList();

			return Ok(new DataSendingResponse
			{
				IsSuccess = true,
				Message = "Pending users retrieved successfully.",
				Data = result
			});
		}

		[HttpPost("approve-user/{guid}")]
		public async Task<ActionResult<DataSendingResponse>> ApproveUser(string guid)
		{
			var user = await _userManager.Users.FirstOrDefaultAsync(user => user.Id.ToString() == guid);
			var email = user.Email;

			if (user == null)
			{
				return NotFound(new DataSendingResponse
				{
					IsSuccess = false,
					Message = "User not found.",
					Data = null
				});
			}

			user.Status = ProofStatus.Verified;

			var result = await _userManager.UpdateAsync(user);
			var tourguide = await _context.TourGuide.FirstOrDefaultAsync(tg => tg.User.Id.ToString() == guid);
			tourguide.IsAvailable = true;
			_context.Update(tourguide);
			if (result.Succeeded)
			{
				var sent = await _userService.SendAccountApprovedEmail(email);
				if (sent.IsSuccess)
				{
					return Ok(new DataSendingResponse
					{
						IsSuccess = true,
						Message = "User approved successfully.",
						Data = null
					});
				}
				return Ok(new DataSendingResponse
				{
					IsSuccess = true,
					Message = "Failed to send email",
					Data = null
				});
			}

			return BadRequest(new DataSendingResponse
			{
				IsSuccess = false,
				Message = "Failed to approve user.",
				Errors = result.Errors.Select(e => e.Description)
			});
		}

		[HttpPost("reject-user/{guid}")]
		public async Task<ActionResult<DataSendingResponse>> RejectUser(string guid)
		{
			var user = await _userManager.Users.FirstOrDefaultAsync(user => user.Id.ToString() == guid);
			var email = user.Email;

			if (user == null)
			{
				return NotFound(new DataSendingResponse
				{
					IsSuccess = false,
					Message = "User not found.",
					Data = null
				});
			}

			var result = await _userManager.DeleteAsync(user);

			if (result.Succeeded)
			{
				var sent = await _userService.SendAccountRejectedEmail(email);
				if (sent.IsSuccess)
				{
					return Ok(new DataSendingResponse
					{
						IsSuccess = true,
						Message = "User rejected and removed successfully.",
						Data = null
					});
				}
				return Ok(new DataSendingResponse
				{
					IsSuccess = true,
					Message = "User Removed but Failed to send email",
					Data = null
				});
			}

			return BadRequest(new DataSendingResponse
			{
				IsSuccess = false,
				Message = "Failed to reject and remove user.",
				Errors = result.Errors.Select(e => e.Description)
			});
		}

		[HttpGet("get-users")]
		public async Task<ActionResult<DataSendingResponse>> GetAllUsers()
		{
			var users = await _userManager.Users.Where(u => u.Role != "Admin" && u.Status == ProofStatus.Verified).Select(user =>
				new AllUsersAdminViewDto
				{
					UserId = user.Id,
					FullName = user.FullName,
					Email = user.Email,
					Role = user.Role,
				}).ToListAsync();

			if (!users.Any())
			{
				return NotFound(new DataSendingResponse
				{
					IsSuccess = false,
					Message = "No users found.",
					Data = null
				});
			}

			return Ok(new DataSendingResponse
			{
				IsSuccess = true,
				Message = "All users retrieved successfully.",
				Data = users
			});
		}

		[HttpDelete("delete-user/{guid}")]
		public async Task<ActionResult<DataSendingResponse>> DeleteUser(string guid)
		{
			var user = await _userManager.FindByIdAsync(guid);
			if (user == null)
			{
				return NotFound(new DataSendingResponse
				{
					IsSuccess = false,
					Message = "User not found.",
					Data = null
				});
			}

			var result = await _userManager.DeleteAsync(user);
			if (result.Succeeded)
			{
				return Ok(new DataSendingResponse
				{
					IsSuccess = true,
					Message = "User deleted successfully.",
					Data = null
				});
			}

			return BadRequest(new DataSendingResponse
			{
				IsSuccess = false,
				Message = "Failed to delete user.",
				Errors = result.Errors.Select(e => e.Description)
			});
		}

		[HttpGet("get-tourists")]
		public async Task<ActionResult<DataSendingResponse>> GetTourists()
		{
			var tourists = await _userManager.Users.Where(u => u.Role == "Tourist").Select(user =>
				new AllUsersAdminViewDto
				{
					UserId = user.Id,
					FullName = user.FullName,
					Email = user.Email,
					Role = user.Role,
				}).ToListAsync();

			if (!tourists.Any())
			{
				return NotFound(new DataSendingResponse
				{
					IsSuccess = false,
					Message = "No tourists found in the database.",
					Data = null
				});
			}

			return Ok(new DataSendingResponse
			{
				IsSuccess = true,
				Message = "Tourists retrieved successfully.",
				Data = tourists
			});
		}

		[HttpGet("stats/last-30-days")]
		public async Task<ActionResult<DataSendingResponse>> GetLast30DaysAddedUsers()
		{
			var thirtyDaysAgo = DateTime.Now.AddDays(-30);
			var sixtyDaysAgo = DateTime.Now.AddDays(-60);

			var currentStats = await _userManager.Users
				.Where(u => u.CreatedAt >= thirtyDaysAgo && u.Role != "Admin" && u.Status == ProofStatus.Verified)
				.GroupBy(u => u.Role)
				.Select(g => new
				{
					Role = g.Key,
					Count = g.Count()
				})
				.ToListAsync();

			var previousStats = await _userManager.Users
				.Where(u => u.CreatedAt >= sixtyDaysAgo && u.CreatedAt < thirtyDaysAgo && u.Role != "Admin")
				.GroupBy(u => u.Role)
				.Select(g => new
				{
					Role = g.Key,
					Count = g.Count()
				})
				.ToListAsync();

			// Merge current and previous stats
			var mergedStats = currentStats.Select(current => new
			{
				Role = current.Role,
				Count = current.Count,
				PreviousCount = previousStats.FirstOrDefault(p => p.Role == current.Role)?.Count ?? 0
			}).ToList();

			if (!mergedStats.Any())
			{
				return NotFound(new DataSendingResponse
				{
					IsSuccess = false,
					Message = "No users added in the last 30 days.",
					Data = null
				});
			}

			return Ok(new DataSendingResponse
			{
				IsSuccess = true,
				Message = "User statistics for the last 30 days retrieved successfully.",
				Data = mergedStats
			});
		}


		[HttpGet("stats/all-time")]
		public async Task<ActionResult<DataSendingResponse>> GetAllTimeUsers()
		{
			// Current stats (all-time user counts)
			var currentStats = await _userManager.Users
				.Where(u => u.Role != "Admin")
				.GroupBy(u => u.Role)
				.Select(g => new
				{
					Role = g.Key,
					Count = g.Count()
				})
				.ToListAsync();

			// Previous stats (e.g., users added until a year ago)
			var oneYearAgo = DateTime.Now.AddYears(-1);
			var previousStats = await _userManager.Users
				.Where(u => u.CreatedAt <= oneYearAgo && u.Role != "Admin" && u.Status == ProofStatus.Verified)
				.GroupBy(u => u.Role)
				.Select(g => new
				{
					Role = g.Key,
					Count = g.Count()
				})
				.ToListAsync();

			// Merge current and previous stats
			var mergedStats = currentStats.Select(current => new
			{
				Role = current.Role,
				Count = current.Count,
				PreviousCount = previousStats.FirstOrDefault(p => p.Role == current.Role)?.Count ?? 0,
				Change = current.Count - (previousStats.FirstOrDefault(p => p.Role == current.Role)?.Count ?? 0)
			}).ToList();

			if (!mergedStats.Any())
			{
				return NotFound(new DataSendingResponse
				{
					IsSuccess = false,
					Message = "No users found in the database.",
					Data = null
				});
			}

			return Ok(new DataSendingResponse
			{
				IsSuccess = true,
				Message = "All-time user statistics retrieved successfully.",
				Data = mergedStats
			});
		}

	}
}
