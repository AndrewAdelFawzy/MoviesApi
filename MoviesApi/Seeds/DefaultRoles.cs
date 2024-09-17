using Microsoft.AspNetCore.Identity;

namespace MoviesApi.Seeds
{
	public class DefaultRoles
	{
		public static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
		{
			if (!roleManager.Roles.Any())
			{
				await roleManager.CreateAsync(new IdentityRole("Admin"));
				await roleManager.CreateAsync(new IdentityRole("User"));
				
			}
		}
	}
}
