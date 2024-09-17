namespace MoviesApi.Helpers
{
	public class JWt
	{
        public string key { get; set; } = null!;
        public string Issuer { get; set; } =null!;
        public string Audience { get; set; }=null!;
        public string DurationInDays { get; set; } = null !;
    }
}
