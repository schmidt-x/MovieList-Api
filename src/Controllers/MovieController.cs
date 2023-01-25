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
	public async Task<IActionResult> GetAllByActor(int id, int? from, int? to, string? orderBy, bool desc)
	{
		try
		{
			var result = await _mService.GetAllByAsync(id, GetBy.Actor, from, to, orderBy, desc);
			
			return result.Count != 0
				? Ok(result)
				: NotFound("Couldn't find any movie");
		}
		catch
		{
			return Problem();
		}
	}
	
	[HttpGet("byGenre/{id:int}")]
	public async Task<IActionResult> GetAllByGenre(int id, int? from, int? to, string? orderBy, bool desc)
	{
		try
		{
			var result = await _mService.GetAllByAsync(id, GetBy.Genre, from, to, orderBy, desc);
			
			return result.Count != 0
				? Ok(result)
				: NotFound("Coulnd't find any movie");
		}
		catch
		{
			return Problem();
		}
	}
	
	[HttpPost]
	public async Task<IActionResult> Save(MoviePost movie, [FromQuery] int[] actorId, [FromQuery] int[] genreId)
	{
		try
		{
			var result = await _mService.SaveAsync(movie, actorId, genreId);
			return Ok(result);
		}
		catch(Exception ex)
		{
			return ex is ArgumentException
				? Problem(ex.Message)
				: Problem();
		}
	}
	
	[HttpDelete]
	public async Task<IActionResult> Delete([FromQuery] int[] id)
	{
		try
		{
			int deletedMovies = await _mService.DeleteAsync(id);
			
			return deletedMovies > 0
				? Ok($"Movies successfully deleted ({deletedMovies})")
				: NotFound("Couldn't find any movie to delete");
		}
		catch
		{
			return Problem();
		}
	}
	
	[HttpPut]
	public Task<IActionResult> Update()
	{
		throw new NotImplementedException();
	}
	
}

/*
	https://localhost:7001/Movie
  
  // GET // 
  
  /1 => get a single movie by id
  
	/ => get all the movies
		? orderBy=(released(int) / title(string) / duration(int)) => sorting the movies
		& desc=true => sort descending 
    & from=2002 => from released date
		& to=2010   => up to released date
		 
	/byActor/3 => get all the movies with specified actor
	/byGenre/3 => get all movies with specified genre
		*the same query params for sorting*
	
	
	// POST //
	
	/ => save the movie
		the movie
		{
			movie's json
		}
		?actorId=1 & actorId=2 => existing actor's ids
		&genreId=1 & genreId=2 => existing genre's ids
		
	// DELETE //
	
	/? id=1 & id=3 => delete the movies
	
	
	// UPDATE //
	
	
	
	
	
*/