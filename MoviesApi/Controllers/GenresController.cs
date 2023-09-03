using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoviesApi.Dtos;
using MoviesApi.Services;

namespace MoviesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenresController : ControllerBase
    {
        private readonly IGenresService _genreService;
        private readonly IMapper _mapper;

        public GenresController(IGenresService genreService, IMapper mapper)
        {
            _genreService = genreService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var genres = await _genreService.GetAll();
            return Ok(genres);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(byte id)
        {
            var genre = await _genreService.GetById(id);
            if (genre is null)
            {
                return NotFound($"No genre was found with ID: {id}");
            }
            return Ok(genre);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(GenreDto dto)
        {
            var genre = _mapper.Map<Genre>(dto);
            await _genreService.Add(genre);
            return Ok(genre);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(byte id, [FromBody] GenreDto dto)
        {
            var genre = await _genreService.GetById(id);
            if (genre is null)
            {
                return NotFound($"No genre was found with ID: {id}");
            }
            genre.Name = dto.Name;
            _genreService.Update(genre);
            return Ok(genre);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(byte id)
        {
            var genre = await _genreService.GetById(id);
            if (genre is null)
            {
                return NotFound($"No genre was found with ID: {id}");
            }
            _genreService.Delete(genre);
            return Ok(genre);
        }
    }
}
