using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoviesApi.Services;

namespace MoviesApi.Controllers
{
    [Route("api/[controller]")]
	[ApiController]
	public class AuthController : ControllerBase
	{
        private readonly IAuthService _authService;

		public AuthController(IAuthService authService )
		{
			_authService = authService;
		}

		[HttpPost("register")]
		public async Task<IActionResult> RegisterAsync([FromForm]RegisterDto model)
		{

			var result = await _authService.RegisterAsync(model);

			if(!result.IsAuthenticated)
				return BadRequest(result.Message);

			return Ok(result);
		}

		[HttpPost("token")]
		public async Task<IActionResult> GetTokenAsync([FromForm] TokenRequestDto model)
		{

			 var result =await _authService.GetTokenAsync(model);

			if(!result.IsAuthenticated)
				return BadRequest(result.Message);

			return Ok(result);

		}

		[HttpPost("assigenRole")]
		public async Task<IActionResult> AssigenRoleAsync([FromForm] AssigenRoleDto model)
		{

			var result = await _authService.AssigenRoleAsync(model);

			if(!string.IsNullOrEmpty(result))
				return BadRequest(result);

			return Ok(model);

		}
	}
}
