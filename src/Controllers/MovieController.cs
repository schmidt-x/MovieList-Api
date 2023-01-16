using Microsoft.AspNetCore.Mvc;
using MovieApi.Models;
using MovieApi.Services;

namespace MovieApi.Controllers;

[ApiController]
[Route("[controller]")]
public class MovieController : ControllerBase
{
	private readonly IMovieService _mService;
	public MovieController(IMovieService mService)
	{
		_mService = mService;
	}
	
	[HttpGet("{id:int}")]
	public async Task<IActionResult> Get(int id)
	{
		try
		{
			var result = await _mService.GetByIdAsync(id);
			
			return result != null
				? Ok(result)
				: NoContent();
		}
		catch
		{
			return Problem("Something went wrong");
		}
		
	}
	
	[HttpGet]
	public async Task<ActionResult<MovieWrap>> GetAll(int? from, int? to, string? orderBy, bool desc)
	{
		var result = await _mService.GetAllAsync(from, to, orderBy, desc);
		
		return result.Count != 0
			? Ok(result)
			: NoContent();
	}
	
	[HttpGet("byActor/{id:int}")]
	public async Task<IActionResult> GetAllByActor(int id, string? orderBy, int? from, int? to, bool desc)
	{
		var result = await _mService.GetAllByAsync(id, GetBy.Actor, from, to, orderBy, desc);
		
		return result.Count != 0
			? Ok(result)
			: NoContent();
	}
	
	[HttpGet("byGenre/{id:int}")]
	public async Task<IActionResult> GetAllByGenre(int id, string? orderBy, int? from, int? to, bool desc)
	{
		try
		{
			var result = await _mService.GetAllByAsync(id, GetBy.Genre, from, to, orderBy, desc);
			
			return result.Count != 0
				? Ok(result)
				: NoContent();
		}
		catch
		{
			return Problem("Something went wrong");
		}
	}
	
	[HttpPost]
	public async Task<IActionResult> Save(MoviePost movie)
	{
		try
		{
			var result = await _mService.SaveMovie(movie);
			return Ok(result);
		}
		catch
		{
			return Problem("Something went wrong");
		}
	}
	
	
}