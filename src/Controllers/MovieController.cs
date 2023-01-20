using Microsoft.AspNetCore.Mvc;
using MovieApi.DTOs;
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
				: NotFound("Couldn't find the movie");
		}
		catch
		{
			return Problem();
		}
	}
	
	[HttpGet]
	public async Task<IActionResult> GetAll(int? from, int? to, string? orderBy, bool desc)
	{
		try
		{
			var result = await _mService.GetAllAsync(from, to, orderBy, desc);
			
			return result.Count != 0
				? Ok(result)
				: NotFound("Couldn't find any movie");
		}
		catch
		{
			return Problem();
		}
	}
	
	[HttpGet("byActor/{id:int}")]
	public async Task<IActionResult> GetAllByActor(int id, string? orderBy, int? from, int? to, bool desc)
	{
		try
		{
			var result = await _mService.GetAllByAsync(id, GetBy.Actor, from, to, orderBy, desc);
			
			return result.Count != 0
				? Ok(result)
				: NotFound("Couldn't find the movies");
		}
		catch
		{
			return Problem();
		}
	}
	
	[HttpGet("byGenre/{id:int}")]
	public async Task<IActionResult> GetAllByGenre(int id, string? orderBy, int? from, int? to, bool desc)
	{
		try
		{
			var result = await _mService.GetAllByAsync(id, GetBy.Genre, from, to, orderBy, desc);
			
			return result.Count != 0
				? Ok(result)
				: NotFound("Coulnd't find the movies");
		}
		catch
		{
			return Problem();
		}
	}
	
	[HttpPost]
	public async Task<IActionResult> Save(MoviePost movie)
	{
		try
		{
			var result = await _mService.SaveAsync(movie);
			return Ok(result);
		}
		catch
		{
			return Problem();
		}
	}
	
	[HttpDelete("{id:int}")]
	public async Task<IActionResult> Delete(int id)
	{
		try
		{
			var isDeleted = await _mService.DeleteAsync(id);
			
			return isDeleted
				? Ok("Movie successfully deleted")
				: NotFound("Couldn't find the movie");
		}
		catch
		{
			return Problem();
		}
	}
	
}