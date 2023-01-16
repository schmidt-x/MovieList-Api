using Microsoft.AspNetCore.Mvc;
using MovieApi.Services;

namespace MovieApi.Controllers;

[ApiController]
[Route("[controller]")]
public class ActorController : ControllerBase
{
	private readonly IActorService _aService;
	public ActorController(IActorService aService)
	{
		_aService = aService;
	}
	
	[HttpGet("Actor")]
	public Task<IActionResult> GetAll()
	{
		// var result = 
		throw new NotImplementedException();
		
	}
}