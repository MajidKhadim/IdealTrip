using IdealTrip.Models;
using IdealTrip.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IdealTrip.Controllers
{
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	[Authorize]
	[Route("api/[controller]")]
	public class UserController : ControllerBase
	{
		private readonly IUserService _userService;
		private readonly IHttpContextAccessor _contextAccessor;
        public UserController(IUserService userService,IHttpContextAccessor httpContextAccessor)
        {
            _userService = userService;
			_contextAccessor = httpContextAccessor;
        }
        [HttpGet]
		public async Task<IActionResult> GetUserProfile()
		{
			try
			{
				var userId = _contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
				if (string.IsNullOrEmpty(userId))
				{
					return Unauthorized("Invalid token.");
				}
				var userInfo = await _userService.GetUserInfo(userId);  // Retrieve user profile from database
				if (userInfo.IsSuccess)
				{
					return Ok(userInfo);
				}
				return BadRequest(userInfo);
				
			}
			catch (Exception ex)
			{
				return StatusCode(500, new UserManagerResponse
				{
					IsSuccess = false,
					Messege = "Something went wrong"!

				});

			}
		}

		[HttpPost]
		public async Task<IActionResult> UpdateUser([FromForm] UpdateUserModel model)
		{
			try
			{
				var userId = _contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
				if (string.IsNullOrEmpty(userId))
				{
					return Unauthorized("Invalid token.");
				}
				var result = await _userService.UpdateUser(model,userId);
				if (result.IsSuccess)
				{
					return Ok(result);
				}
				return BadRequest(result);
			}
			catch (Exception ex) 
			{
				return BadRequest(new UserManagerResponse
				{
					IsSuccess = false,
					Messege = ex.Message
				});
			}
		}
		[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		[Authorize(Roles = "Admin")]
		[HttpGet("{userId}")]
		public async Task<IActionResult> GetUserDetails(string userId)
		{
			try
			{
				var result = await _userService.GetUserDetails(userId);
				if (result.IsSuccess)
				{
					return Ok(result);
				}
				return BadRequest(result);
			}
			catch (Exception ex)
			{
				return StatusCode(505, new UserManagerResponse
				{
					IsSuccess = false,
					Messege = "Internal Server Error",
					Errors = new List<string> { "Internal Server Error" }
				});
			}
		}

	}
}
