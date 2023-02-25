using Microsoft.AspNetCore.Mvc;
using MovieApi.DTOs;
using MovieApi.Services;
using MovieApi.Enums;

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
		var result = await _mService.GetAsync(id);
		
		return result != null
			? Ok(result)
			: NotFound(new { error = "Movie not found" });
	}
	
	[HttpGet]
	public async Task<ActionResult<Wrap<MoviesGet>>> GetAll(int? from, int? to, string? orderBy, bool desc)
	{
		var result = await _mService.GetAllAsync(from, to, orderBy, desc);
		
		return result.Count > 0
			? Ok(result)
			: NotFound("Movies not found");
	}
	
	[HttpGet("byActor/{id:int}")]
	public async Task<ActionResult<Wrap<MoviesGet>>> GetAllByActor(int id, int? from, int? to, string? orderBy, bool desc)
	{
		var result = await _mService.GetAllByAsync(id, GetBy.Actor, from, to, orderBy, desc);
		
		return result.Count != 0
			? Ok(result)
			: NotFound("Movies not found");
	}
	
	[HttpGet("byGenre/{id:int}")]
	public async Task<ActionResult<Wrap<MoviesGet>>> GetAllByGenre(int id, int? from, int? to, string? orderBy, bool desc)
	{
		var result = await _mService.GetAllByAsync(id, GetBy.Genre, from, to, orderBy, desc);
		
		return result.Count > 0
			? Ok(result)
			: NotFound("Movies not found");
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
		await _mService.DeleteAsync(movieId);
		
		return NoContent();
	}
	
	[HttpPut]
	public async Task<ActionResult<MovieGet>> Update(MoviePost movie, [FromQuery] int[] actorIdDel, [FromQuery] int[] genreIdDel, [FromQuery] int[] actorIdAdd, [FromQuery] int[] genreIdAdd)
	{
		var result = await _mService.UpdateAsync(movie, actorIdDel, genreIdDel, actorIdAdd, genreIdAdd);
		
		return Ok(result);
	}
}