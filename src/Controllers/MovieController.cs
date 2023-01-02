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
	
	[HttpGet]
	public async Task<ActionResult<MovieWrap>> GetAll(int? from, int? to, bool unfolded = false, string? orderBy = null, bool desc = false)
	{
		var result = await _mService.GetAllAsync(from, to, unfolded, orderBy, desc);
		
		return result.Count != 0
			? Ok(result)
			: NoContent();
	}
	
	[HttpGet("{title}/{released:int?}")]
	public async Task<IActionResult> Get(string title, int? released = null)
	{
		var result = await _mService.GetSingleAsync(title, released);
		
		return result != null
			? Ok(result)
			: NoContent();
	}
	
	[HttpGet("Actor/{name}")]
	public async Task<IActionResult> GetAllByActor(string name, string? orderBy = null, bool desc = false)
	{
		var result = await _mService.GetAllByAsync(GetBy.Actor, name, orderBy, desc);
		
		return result.Count != 0
			? Ok(result)
			: NoContent();
	}
	
	[HttpGet("Genre/{type}")]
	public async Task<IActionResult> GetAllByGenre(string type, string? orderBy = null, bool desc = false)
	{
		var result = await _mService.GetAllByAsync(GetBy.Genre, type, orderBy, desc);
		
		return result.Count != 0
			? Ok(result)
			: NoContent();
	}
	
	
	
	
	// [HttpPost]
	// public async Task<IActionResult> SaveMovie()
	// {
	// 	
	// 	
	// 	
	// 	throw new NotImplementedException();
	// }
	
}