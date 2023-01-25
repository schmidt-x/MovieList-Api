using Microsoft.AspNetCore.Mvc;
using MovieApi.DTOs;
using MovieApi.Services;
using MovieApi.SqlQueries;

namespace MovieApi.Controllers;

[ApiController]
[Route("[controller]")]
public class ActorController : ControllerBase
{
	private readonly IMemberService _membersService;
	public ActorController(IMemberService membersService) =>
		_membersService = membersService;
	
	[HttpGet]
	public async Task<IActionResult> GetAll()
	{
		try
		{
			var result = await _membersService.GetAllAsync<ActorGet>(ActorSql.GetAll);
			
			return result.Count != 0
				? Ok(result)
				: NotFound("Couldn't find any actor");
		}
		catch
		{
			return Problem();
		}
	}
	
	[HttpDelete]
	public async Task<IActionResult> Delete([FromQuery] int[] id)
	{
		try
		{
			int deletedActors = await _membersService.DeleteAsync(id, ActorSql.Delete);
			return deletedActors > 0
				? Ok($"Actors deleted ({deletedActors})")
				: NotFound("Couldn't find any actor to delete");
		}
		catch
		{
			return Problem();
		}
	}
}