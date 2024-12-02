using IdealTrip.Models;
using IdealTrip.Models.AdminView;
using IdealTrip.Models.Enums;
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
	[Authorize("Admin")]
	[Route("api/[controller]")]
	public class AdministrationController : Controller
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly ApplicationDbContext _context;

		public AdministrationController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
		{
			_userManager = userManager;
			_context = context;
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
		public async Task<ActionResult<DataSendingResponse>> ApproveUser(Guid guid)
		{
			var user = await _userManager.Users.FirstOrDefaultAsync(user => user.Id == guid);

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

			if (result.Succeeded)
			{
				return Ok(new DataSendingResponse
				{
					IsSuccess = true,
					Message = "User approved successfully.",
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
		public async Task<ActionResult<DataSendingResponse>> RejectUser(Guid guid)
		{
			var user = await _userManager.Users.FirstOrDefaultAsync(user => user.Id == guid);

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
					Message = "User rejected and removed successfully.",
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
			var users = await _userManager.Users.Where(u => u.Role != "Admin").Select(user =>
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

		[HttpPost("delete-user/{guid}")]
		public async Task<ActionResult<DataSendingResponse>> DeleteUser(Guid guid)
		{
			var user = await _userManager.FindByIdAsync(guid.ToString());
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

			var stats = await _userManager.Users
				.Where(u => u.CreatedAt >= thirtyDaysAgo && u.Role != "Admin")
				.GroupBy(u => u.Role)
				.Select(g => new
				{
					Role = g.Key,
					Count = g.Count()
				})
				.ToListAsync();

			if (!stats.Any())
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
				Data = stats
			});
		}

		[HttpGet("stats/all-time")]
		public async Task<ActionResult<DataSendingResponse>> GetAllTimeUsers()
		{
			var stats = await _userManager.Users
				.Where(u => u.Role != "Admin")
				.GroupBy(u => u.Role)
				.Select(g => new
				{
					Role = g.Key,
					Count = g.Count()
				})
				.ToListAsync();

			if (!stats.Any())
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
				Data = stats
			});
		}
	}
}
