using Microsoft.AspNetCore.Mvc;
using MovieApi.DTOs;
using MovieApi.Services;
using MovieApi.SqlQueries;

namespace MovieApi.Controllers;

[ApiController]
[Route("[controller]")]
public class GenreController : ControllerBase
{
	private readonly IMemberService _membersService;
	public GenreController(IMemberService membersService) =>
		_membersService = membersService;
	
	[HttpGet]
	public async Task<IActionResult> GetAll()
	{
		try
		{
			var result = await _membersService.GetAllAsync<GenreGet>(GenreSql.GetAll);
			
			return result.Count != 0
				? Ok(result)
				: NotFound("Couldn't find any genre");
		}
		catch
		{
			return Problem();
		}
	}
	
	
	
}