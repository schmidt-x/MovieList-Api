using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MovieApi.ActionResults;

class DuplicateActionResult : IActionResult
{
	private readonly int _id;
	private readonly string _arg;
	
	public DuplicateActionResult(int id, string arg)
	{
		_id = id;
		_arg = arg;
	}
	
	public Task ExecuteResultAsync(ActionContext context)
	{
		var request = context.HttpContext.Request;
		var response = context.HttpContext.Response;
		
		response.StatusCode = 400;
		var url = $"{request.Scheme}://{request.Host}{request.Path}/{_id}";
		response.Headers.Location = url;
		
		return response.WriteAsJsonAsync(new { error = $"'{_arg}' already exists!"} );
	}
}