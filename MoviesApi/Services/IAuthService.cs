namespace MoviesApi.Services
{
    public interface IAuthService
	{
		Task<AuthModel> RegisterAsync(RegisterDto model);
		Task<AuthModel> GetTokenAsync(TokenRequestDto model);
		Task<string> AssigenRoleAsync(AssigenRoleDto model);
	}
}
