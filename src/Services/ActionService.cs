using Microsoft.AspNetCore.Mvc;
using MovieApi.Actions;

namespace MovieApi.Services;

public class ActionService
{
	public IActionResult Created(int id, string arg)
	{
		bool isDuplicate = id < 0;
		
		if (isDuplicate)
			id *= -1;
		
		return isDuplicate
			? new DuplicateActionResult(id, arg)
			: new CreatedActionResult(id, arg);
	}
}