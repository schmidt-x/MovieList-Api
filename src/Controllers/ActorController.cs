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
	
	[HttpDelete("{id:int}")]
	public async Task<IActionResult> Delete(int id)
	{
		try
		{
			var isDeleted = await _membersService.DeleteAsync(id, ActorSql.Delete);
			return isDeleted
				? Ok("Actor deleted")
				: NotFound("Couldn't find the actor");
		}
		catch
		{
			return Problem();
		}
		
	}
}