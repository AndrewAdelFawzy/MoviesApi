
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MoviesApi.Helpers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MoviesApi.Services
{
	public class AuthService : IAuthService
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly JWT _jwt;

		public AuthService(UserManager<ApplicationUser> userManager, IOptions<JWT> jwt)
		{
			_userManager = userManager;
			_jwt = jwt.Value;
		}

		public async Task<AuthModel> RegisterAsync(RegisterModel model)
		{
			if(await _userManager.FindByEmailAsync(model.Email) is not null)
				return new AuthModel{ Message="This email is alread exist"};

			if (await _userManager.FindByNameAsync(model.Username) is not null)
				return new AuthModel { Message = "This UserName is alread exist" };

			var user = new ApplicationUser()
			{
				UserName = model.Username,
				Email = model.Email,
				FirstName = model.FirstName,
				LastName = model.LastName,
			};

			var result =await _userManager.CreateAsync(user,model.Password);

			if (!result.Succeeded)
			{
				var errors = string.Empty;

				foreach (var error in result.Errors)
				{
					errors += $"{error.Description}";
				}

				return new AuthModel { Message = errors };
			}

			await _userManager.AddToRoleAsync(user, "User");

			var jwtSecurityToken = await CreateJwtToken(user);

			return new AuthModel
			{
				Email = user.Email,
				ExpiresOn = jwtSecurityToken.ValidTo,
				IsAuthenticated = true,
				Roles = new List<string> { "User" },
				Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
				UserName = user.UserName
			};
		}

		private async Task<JwtSecurityToken> CreateJwtToken(ApplicationUser user)
		{
			var userClaims = await _userManager.GetClaimsAsync(user);
			var roles = await _userManager.GetRolesAsync(user);
			var roleClaims = new List<Claim>();

			foreach (var role in roles)
				roleClaims.Add(new Claim("roles", role));

			var claims = new[]
			{
				new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
				new Claim(JwtRegisteredClaimNames.Email, user.Email),
				new Claim("uid", user.Id)
			}
			.Union(userClaims)
			.Union(roleClaims);

			var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.key));
			var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

			var jwtSecurityToken = new JwtSecurityToken(
				issuer: _jwt.Issuer,
				audience: _jwt.Audience,
				claims: claims,
				expires: DateTime.Now.AddDays(_jwt.DurationInDays),
				signingCredentials: signingCredentials);

			return jwtSecurityToken;
		}
	}
}
