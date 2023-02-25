﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MovieApi.Actions;

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
		
		return response.WriteAsJsonAsync(new { error = $"Item '{_arg}' already exists!"} );
	}
}

class CreatedActionResult : IActionResult
{
	private readonly int _id;
	private readonly string _arg;
	
	public CreatedActionResult(int id, string arg)
	{
		_id = id;
		_arg = arg;
	}
	
	public Task ExecuteResultAsync(ActionContext context)
	{
		var request = context.HttpContext.Request;
		var response = context.HttpContext.Response;
		
		response.StatusCode = 201;
		var location = $"{request.Scheme}://{request.Host}{request.Path}/{_id}";
		response.Headers.Location = location;
		
		return response.WriteAsJsonAsync(new { message = $"Item '{_arg}' successfully created!"}); 
	}
}