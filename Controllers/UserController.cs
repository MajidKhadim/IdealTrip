using IdealTrip.Models;
using IdealTrip.Services;
using Microsoft.AspNetCore.Mvc;

namespace IdealTrip.Controllers
{
	[Route("api/[controller]")]
	public class UserController : ControllerBase
	{
		private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }
        [HttpGet("{userId}")]
		public async Task<IActionResult> GetUserProfile(string userId)
		{
			try
			{
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

		[HttpPost("{id}")]
		public async Task<IActionResult> UpdateUser(string id, [FromForm] UpdateUserModel model)
		{
			try
			{
				var result = await _userService.UpdateUser(model,id);
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
	}
}
