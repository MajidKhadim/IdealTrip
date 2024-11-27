using IdealTrip.Models;
using Microsoft.AspNetCore.Identity;

namespace IdealTrip.Helpers
{
	public class NonUniqueUserNameValidator : UserValidator<ApplicationUser>
	{
		public override async Task<IdentityResult> ValidateAsync(UserManager<ApplicationUser> manager, ApplicationUser user)
		{
			var errors = new List<IdentityError>();

			// Skip UserName uniqueness validation
			var result = await base.ValidateAsync(manager, user);
			foreach (var error in result.Errors)
			{
				if (!error.Code.Contains("DuplicateUserName"))
				{
					errors.Add(error);
				}
			}

			return errors.Count > 0 ? IdentityResult.Failed(errors.ToArray()) : IdentityResult.Success;
		}
	}

}
