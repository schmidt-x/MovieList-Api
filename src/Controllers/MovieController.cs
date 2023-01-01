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
	public async Task<ActionResult<MovieWrap>> GetAll(int? from, int? to, bool folded = false)
	{
		var result = await _mService.GetAllAsync(from, to, folded);
		
		return result.Count != 0
			? Ok(result)
			: NoContent();
	}
	
	[HttpGet("{name}/{released:int?}")]
	public async Task<ActionResult<MovieGet>> Get(string name, int? released)
	{
		var result = await _mService.GetSingleAsync(name, released);
		
		return result.Count != 0
			? Ok(result.Movies.First())
			: NoContent();
	}
	
	// [HttpGet("Actor/{name}")]
	// public async Task<ActionResult<MovieWrap>> GetBy(string name)
	// {
		
		
		
		// throw new NotImplementedException();
	// }
	
	
}