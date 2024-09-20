
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
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly JWT _jwt;

		public AuthService(UserManager<ApplicationUser> userManager, IOptions<JWT> jwt,RoleManager<IdentityRole> roleManager)
		{
			_userManager = userManager;
			_jwt = jwt.Value;
			_roleManager = roleManager;
		}

		public async Task<AuthModel> RegisterAsync(RegisterDto model)
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

		public async Task<AuthModel> GetTokenAsync(TokenRequestDto model)
		{
			AuthModel authModel = new AuthModel();

			var user = await _userManager.FindByEmailAsync(model.Email);

			if (user is null || !await _userManager.CheckPasswordAsync(user,model.Password))
			{
				authModel.Message = "Email or Password is incorrect";
				return authModel;
			}

			var jwtSecurityToken = await CreateJwtToken(user);
			var rolesList = await _userManager.GetRolesAsync(user);

			authModel.IsAuthenticated = true;
			authModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
			authModel.UserName = user.UserName;
			authModel.Roles = rolesList.ToList();
			authModel.Email = user.Email;
			authModel.ExpiresOn = jwtSecurityToken.ValidTo;
		
			return  authModel;
		}


		public async Task<string> AssigenRoleAsync(AssigenRoleDto model)
		{
			var user = await _userManager.FindByIdAsync(model.UserId);

			if(user is null || !await _roleManager.RoleExistsAsync(model.RoleName))
				return "Invalid userId or Role Name";

			if (await _userManager.IsInRoleAsync(user, model.RoleName))
				return "User already in this role";

			var result = await _userManager.AddToRoleAsync(user, model.RoleName);

			return result.Succeeded ? string.Empty : "somethimg went wrong";
			
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
