using Microsoft.AspNetCore.Mvc;
using MovieApi.DTOs;
using MovieApi.Repositories;
using MovieApi.Services;

namespace MovieApi.Controllers;

[ApiController]
[Route("[controller]")]
public class ActorController : ControllerBase
{
	private readonly IActorService _aService;
	private readonly ActionService _action;
	public ActorController(IActorService aService, ActionService action)
	{
		_aService = aService;
		_action = action;
	}
	
	[HttpGet("{id:int}")]
	public async Task<ActionResult<ActorGet>> Get(int id)
	{
		var result = await _aService.GetAsync(id);
		
		return result != null
			? Ok(result)
			: NotFound(new { error = "Actor not found" });
	}
	
	[HttpGet]
	public async Task<ActionResult<Wrap<ActorGet>>> GetAll()
	{
		var actors = await _aService.GetAllAsync();
		
		return actors.Count > 0
			? Ok(actors)
			: NotFound(new { error = "Actor(s) not found" });
	}
	
	[HttpPost]
	public async Task<IActionResult> Save(ActorPost actor)
	{
		var id = await _aService.SaveAsync(actor);
		
		return _action.Created(id, actor.Name!);
	}
	
	
}