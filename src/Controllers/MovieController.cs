using Microsoft.AspNetCore.Mvc;
using MovieApi.DTOs;
using MovieApi.Repositories;
using MovieApi.Enums;
using MovieApi.Services;

namespace MovieApi.Controllers;

[ApiController]
[Route("[controller]")]
public class MovieController : ControllerBase
{
	private readonly IMovieService _mService;
	private readonly ActionService _action;
	public MovieController(IMovieService mService, ActionService action)
	{
		_action = action;
		_mService = mService;
	}
	
	[HttpGet("{id:int}")]
	public async Task<ActionResult<MovieGet>> Get(int id)
	{
		var movie = await _mService.GetAsync(id);
		
		return movie != null
			? Ok(movie)
			: NotFound(new { error = "Movie not found" });
	}
	
	[HttpGet]
	public async Task<ActionResult<Wrap<MoviesGet>>> GetAll(int? from, int? to, string? orderBy, bool desc)
	{
		var movies = await _mService.GetAllAsync(from, to, orderBy, desc);
		
		return movies.Count > 0
			? Ok(movies)
			: NotFound(new { error = "Movies not found" });
	}
	
	[HttpGet("byActor/{id:int}")]
	public async Task<ActionResult<Wrap<MoviesGet>>> GetAllByActor(int id, int? from, int? to, string? orderBy, bool desc)
	{
		var movies = await _mService.GetAllByAsync(id, GetBy.Actor, from, to, orderBy, desc);
		
		return movies.Count != 0
			? Ok(movies)
			: NotFound(new { error = "Movies not found" });
	}
	
	[HttpGet("byGenre/{id:int}")]
	public async Task<ActionResult<Wrap<MoviesGet>>> GetAllByGenre(int id, int? from, int? to, string? orderBy, bool desc)
	{
		var movies = await _mService.GetAllByAsync(id, GetBy.Genre, from, to, orderBy, desc);
		
		return movies.Count > 0
			? Ok(movies)
			: NotFound(new { error = "Movies not found" });
	}
	
	[HttpPost]
	public async Task<IActionResult> Save(MoviePost movie, [FromQuery] int[] actorId, [FromQuery] int[] genreId)
	{
		var id = await _mService.SaveAsync(movie, actorId, genreId);
		
		return _action.Created(id, movie.Title!);
	}
	
	[HttpDelete]
	public async Task<IActionResult> Delete([FromQuery] int[] movieId)
	{
		var isDeleted = await _mService.DeleteAsync(movieId);
		
		return isDeleted
			? NoContent()
			: NotFound(new { error = "Movie(s) not found" });
	}
	
	[HttpPut("{id:int}")]
	public async Task<ActionResult<MovieGet>> Update(int id, MoviePut movie, [FromQuery] int[] actorIdDel, [FromQuery] int[] genreIdDel, [FromQuery] int[] actorIdAdd, [FromQuery] int[] genreIdAdd)
	{
		bool isUpdated = await _mService.UpdateAsync(id, movie, actorIdDel, genreIdDel, actorIdAdd, genreIdAdd);
		
		return isUpdated
			? Ok()
			: NotFound(new { error = "Movie not found" });
	}
}