using IdealTrip.Models;
using IdealTrip.Models.AdminView;
using IdealTrip.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace IdealTrip.Controllers
{
    [ApiController]
	[Route("api/[controller]")]
	public class AdministrationController : ControllerBase
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly ApplicationDbContext _context;

		public AdministrationController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
		{
			_userManager = userManager;
			_context = context;
		}

		[HttpGet("pending-users")]
		public async Task<ActionResult<List<PendingUsersAdminViewDto>>> GetPendingProofsUsers()
		{
			var pendingUsers = await _userManager.Users
				.Where(user => user.Status == ProofStatus.Pending)
				.ToListAsync();

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

			return Ok(result);
		}

		[HttpPost("approve-user/{guid}")]
		public async Task<ActionResult> ApproveUser(Guid guid)
		{
			// Find the user by GUID
			var user = await _userManager.Users
				.FirstOrDefaultAsync(user => user.Id == guid);

			// Check if the user exists
			if (user == null)
			{
				return NotFound(new { Message = "User not found." });
			}

			// Update the user's status
			user.Status = ProofStatus.Verified;

			// Persist changes to the database
			var result = await _userManager.UpdateAsync(user);

			if (result.Succeeded)
			{
				return Ok(new { Message = "User approved successfully." });
			}

			// Handle errors
			return BadRequest(new { Message = "Failed to approve user.", Errors = result.Errors });
		}

		[HttpPost("reject-user/{guid}")]
		public async Task<ActionResult> RejectUser(Guid guid)
		{
			// Find the user by GUID
			var user = await _userManager.Users
				.FirstOrDefaultAsync(user => user.Id == guid);

			// Check if the user exists
			if (user == null)
			{
				return NotFound(new { Message = "User not found." });
			}

			// Persist changes to the database
			var result = await _userManager.DeleteAsync(user);

			if (result.Succeeded)
			{
				return Ok(new { Message = "User has been removed successfully." });
			}

			// Handle errors
			return BadRequest(new { Message = "Failed to reject user.", Errors = result.Errors });
		}


		[HttpGet("get-users")]
		public async Task<ActionResult<List<AllUsersAdminViewDto>>> GetAllUsers()
		{
			var model = await _userManager.Users.Select(user 
				=> new AllUsersAdminViewDto
				{
					UserId = user.Id,
					FullName= user.FullName,
					Email = user.Email,
					Role = user.Role,
				}
			).ToListAsync();
			return Ok(model);
		}

		[HttpPost("delete-user")]
		public async Task<ActionResult> DeleteUser(Guid guid)
		{
			var user = await _userManager.FindByIdAsync(guid.ToString());
			if(user == null)
			{
				return NotFound(new {Messege = "User not Found."});
			}
			var result = await _userManager.DeleteAsync(user);
			if (result.Succeeded)
			{
				return Ok(new {Messege = "user deletedd successfully."});
			}
			return BadRequest(new { Messege = "Failed to delete user." });
		}
	}
}
