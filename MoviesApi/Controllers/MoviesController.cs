using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MoviesApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class MoviesController : ControllerBase
	{
		private readonly ApplicationDbContext _context;

		private List<string> _allowedExtentions = new() { ".jpg", ".jpeg", ".png" };
		private long _maxAllowedPosterSize = 1048576;

		public MoviesController(ApplicationDbContext context)
		{
			_context = context;
		}

		[HttpGet]
		public async Task<IActionResult> GetAllAsync()
		{
			var movies = await _context
				.Movies
				.Include(g=>g.Genre)
				.OrderByDescending(g=>g.Rate)
				.ToListAsync();

			if (movies is null)
				return BadRequest("No Movies Found");
			
			return Ok(movies);
		}

		[HttpGet("{Id}")]
		public async Task<IActionResult> GetByIdAsync(int Id)
		{
			var movie = await _context
				.Movies
				.Include(g=>g.Genre)
				.SingleOrDefaultAsync(m=>m.Id==Id);

			if (movie is null)
				return BadRequest("No Movie Found");

			return Ok(movie);
		}

		[HttpGet("GetByGenreId")]
		public async Task<IActionResult> GetByGenreIdAsync(byte GenreId)
		{
			var movie = await _context
				.Movies
				.Where(m => m.GenreId == GenreId)
				.Include(g => g.Genre)
				.OrderByDescending(g => g.Rate)
				.ToListAsync();

			if (movie is null)
				return BadRequest("No Movie Found");

			return Ok(movie);
		}


		[HttpPost]
		public async Task<IActionResult> CreatAsync([FromForm]MovieDto dto)
		{
			if (dto.Poster is null)
				return BadRequest("Poster is required");

			if (!_allowedExtentions.Contains(Path.GetExtension(dto.Poster.FileName).ToLower()))
				return BadRequest("only .png , .jpg and .jpeg is allowed");

			if(dto.Poster.Length >_maxAllowedPosterSize)
				return BadRequest("file size not allowed");

			var isValidGenre = await _context.Genres.AnyAsync(g=>g.Id==dto.GenreId);

			if (!isValidGenre)
				return BadRequest("This genre id is not exist!");

			using var dataStream = new MemoryStream();

			await dto.Poster.CopyToAsync(dataStream);

			Movie movie = new()
			{
				Title = dto.Title,
				Year = dto.Year,
				Rate = dto.Rate,
				Storeline = dto.Storeline,
				Poster =dataStream.ToArray(),
				GenreId = dto.GenreId,
			};

			await _context.AddAsync(movie);
			_context.SaveChanges();

			return Ok(movie);

		}

		[HttpPut("{Id}")]
		public async Task<IActionResult> UpdateAsync(int Id,[FromForm]MovieDto dto)
		{
			var movie = await _context.Movies.FindAsync(Id);

			if (movie is null)
				return NotFound("no movie foumd");


			var isValidGenre = await _context.Genres.AnyAsync(g => g.Id == dto.GenreId);

			if (!isValidGenre)
				return BadRequest("This genre id is not exist!");

			if (dto.Poster != null)
			{
				if (!_allowedExtentions.Contains(Path.GetExtension(dto.Poster.FileName).ToLower()))
					return BadRequest("only .png , .jpg and .jpeg is allowed");

				if (dto.Poster.Length > _maxAllowedPosterSize)
					return BadRequest("file size not allowed");

				using var dataStream = new MemoryStream();

				await dto.Poster.CopyToAsync(dataStream);

				movie.Poster = dataStream.ToArray();
			}


			movie.Title = dto.Title;
			movie.Year = dto.Year;
			movie.Rate = dto.Rate;
			movie.Storeline = dto.Storeline;
			movie.GenreId = dto.GenreId;
			
			_context.SaveChanges();

			return Ok(movie);
		}

		[HttpDelete("{Id}")]
		public async Task<IActionResult> DeleteAsync(int Id)
		{
			var movie = await _context.Movies.FindAsync(Id);

			if (movie is null)
				return NotFound("no movie found");
			
			_context.Movies.Remove(movie);
			_context.SaveChanges();

			return Ok(movie);


		}
		
	}
}
