using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoviesApi.Dtos;
using MoviesApi.Services;

namespace MoviesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly IMoviesService _moviesServices;
        private readonly IGenresService _genresServices;
        private readonly IMapper _mapper;

        private new List<string> _allowedExtensions = new List<string> { ".jpg", ".png" };
        private long _maxAllowedPosterSize = 1048576;

        public MoviesController(IMoviesService moviesServices, IGenresService genresServices, IMapper mapper)
        {
            _moviesServices = moviesServices;
            _genresServices = genresServices;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var movies = await _moviesServices.GetAll();
            var data = _mapper.Map<IEnumerable<MovieDetailsDto>>(movies);
            return Ok(data);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var movie = await _moviesServices.GetById(id);
            if (movie is null)
            {
                return NotFound();
            }

            var dto = _mapper.Map<MovieDetailsDto>(movie);
            return Ok(dto);
        }

        [HttpGet("GetByGenreId")]
        public async Task<IActionResult> GetByGenreIdAsync(byte gereId)
        {
            var movie = await _moviesServices.GetAll(gereId);
            var data = _mapper.Map<IEnumerable<MovieDetailsDto>>(movie);
            return Ok(data);
        }

        //TODO: Post and Update Movie here
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] MovieDto dto)
        {
            if (dto.Poster == null)
                return BadRequest("Poster is required!");

            if (!_allowedExtensions.Contains(Path.GetExtension(dto.Poster.FileName).ToLower()))
                return BadRequest("Only .png and .jpg images are allowed!");

            if (dto.Poster.Length > _maxAllowedPosterSize)
                return BadRequest("Max allowed size for poster is 1MB!");

            var isValidGenre = await _genresServices.IsValidGenre(dto.GenreId);
            if (!isValidGenre)
            {
                return BadRequest("Invalid Genere ID!");
            }

            using var dataStream = new MemoryStream();

            await dto.Poster.CopyToAsync(dataStream);

            var movie = _mapper.Map<Movie>(dto);
            movie.Poster = dataStream.ToArray();

            _moviesServices.Add(movie);
            return Ok(movie);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] MovieDto dto)
        {
            var movie = await _moviesServices.GetById(id);
            if (movie is null)
            {
                return NotFound($"No movie was found with ID: {id}");
            }

            var isValidGenre = await _genresServices.IsValidGenre(dto.GenreId);

            if (!isValidGenre)
                return BadRequest("Invalid genere ID!");

            if (dto.Poster is not null)
            {
                if (!_allowedExtensions.Contains(Path.GetExtension(dto.Poster.FileName).ToLower()))
                    return BadRequest("Only .png and .jpg images are allowed!");

                if (dto.Poster.Length > _maxAllowedPosterSize)
                    return BadRequest("Max allowed size for poster is 1MB!");

                using var dataStream = new MemoryStream();

                await dto.Poster.CopyToAsync(dataStream);

                movie.Poster = dataStream.ToArray();
            }
            movie.Title = dto.Title;
            movie.GenreId = dto.GenreId;
            movie.Year = dto.Year;
            movie.Storeline= dto.Storeline;
            movie.Rate = dto.Rate;

            _moviesServices.Update(movie);
            return Ok(movie);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var movie = await _moviesServices.GetById(id);
            if (movie is null)
            {
                return NotFound($"No movie was found with ID {id}");
            }
            _moviesServices.Delete(movie);
            return Ok(movie);
        }
    }
}
