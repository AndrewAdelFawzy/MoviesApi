namespace MoviesApi.Models
{
	public class RegisterModel
	{

		[Required, StringLength(100)]
		public string FirstName { get; set; } = null!;

		[Required, StringLength(100)]
		public string LastName { get; set; } =null!;

		[Required, StringLength(50)]
		public string Username { get; set; } =null !;

		[Required, StringLength(128)]
		public string Email { get; set; } = null !;

		[Required, StringLength(256)]
		public string Password { get; set; } = null!;
	}
}
