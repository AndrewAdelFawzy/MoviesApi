using Microsoft.AspNetCore.Http;



namespace MoviesApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class GenresController : ControllerBase
	{
		private readonly ApplicationDbContext _context;

		public GenresController(ApplicationDbContext context)
		{
			_context = context;
		}

		[HttpGet]
		public async Task <IActionResult> GetAllAsync()
		{
			var genres = await _context.Genres.OrderBy(g=>g.Name).ToListAsync();

			return Ok(genres);
		}

		[HttpPost]
		public async Task<IActionResult> CreateAsync(GenreDto dto)
		{

			Genre genre = new()
			{
				Name = dto.Name,
			};

			await _context.Genres.AddAsync(genre);
			_context.SaveChanges();

			return Ok(genre);

		}

		[HttpPut("{Id}")]
		public async Task<IActionResult> UpdateAsync(int Id, GenreDto dto)
		{
			var genre = await _context.Genres.SingleOrDefaultAsync(g => g.Id == Id);

			if (genre == null)
				return NotFound($"No Genre with the Id : {Id} is found");

			genre.Name = dto.Name;
			_context.SaveChanges();

			return Ok(genre);
		}

		[HttpDelete("{Id}")]
		public async Task<IActionResult> DeleteAsync(int Id)
		{
			var genre = await _context.Genres.SingleOrDefaultAsync(g => g.Id == Id);

			if (genre == null)
				return NotFound($"No Genre with the Id : {Id} is found");

			_context.Remove(genre);
			_context.SaveChanges();

			return Ok("Removed");
		}
	}
}
