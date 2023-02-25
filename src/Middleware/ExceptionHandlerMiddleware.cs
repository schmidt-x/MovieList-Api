using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MovieApi.Middleware;

public class ExceptionHandlerMiddleware
{
	private RequestDelegate _next;
	private ILogger<ExceptionHandlerMiddleware> _logger;

	public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
	{
		_next = next;
		_logger = logger;
	}
	
	public async Task InvokeAsync(HttpContext context)
	{
		Task? handler = null;
		
		try
		{
			await _next(context);
		}
		catch(Exception ex)
		{
			handler = HandleExceptionAsync(ex, context);
		}
		
		if (handler != null)
			await handler;
		
	}
	
	private async Task HandleExceptionAsync(Exception ex, HttpContext context)
	{
		var response = context.Response;
		string errorMessage;
		
		switch (ex)
		{
			case ArgumentException argExc:
				response.StatusCode = 400;
				errorMessage = ex.Message;
				_logger.LogError(argExc, "Invalid argument {param}", argExc.ParamName);
				break;
			default:
				response.StatusCode = 500;
				errorMessage = "Unexpected error";
				_logger.LogError(ex, errorMessage);
				break;
		}
		await context.Response.WriteAsJsonAsync(new { error = errorMessage });
	}
}

public static class ExceptionHandlerMiddlewareExtensions
{
	public static IApplicationBuilder UseExceptionHandlerMiddleware(this IApplicationBuilder app)
	{
		return app.UseMiddleware<ExceptionHandlerMiddleware>();
	}
}