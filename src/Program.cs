using Microsoft.AspNetCore.Http;
using MovieApi.Models;
using MovieApi.Services;

namespace MovieApi;

public class Program
{
	public static void Main()
	{
		var builder = WebApplication.CreateBuilder();
		builder.Services.AddSingleton<IService, MovieService>();
		
		var app = builder.Build();
		var service = app.Services.GetRequiredService<IService>();
		
		app.MapGet("/Movies", async (ctx) =>
		{
			var result = await service.GetMovies();
			
			if (!result.Any())
			{
				ctx.Response.StatusCode = 404;
				await ctx.Response.WriteAsync("Movies were not found");
				return;
			}
			await ctx.Response.WriteAsJsonAsync(result);
		});
		
		app.MapGet("/Movies/{name}/{age:int?}", async (HttpContext ctx, string name, int? age) =>
		{
			var result = await service.GetMovies(name, age); // the use of the same method that returns all the movies
			
			if (!result.Any())
			{
				ctx.Response.StatusCode = 404;
				await ctx.Response.WriteAsync("Movie was not found");
				return;
			}
			await ctx.Response.WriteAsJsonAsync(result.First());
		});
		
		app.MapGet("/Movies/Actor/{name}", async (HttpContext ctx, string name) =>
		{
			var result = await service.GetMoviesBy(GetByArg.Actor, name);
			
			if (!result.Any())
			{
				ctx.Response.StatusCode = 404;
				await ctx.Response.WriteAsync("Actor was not found");
				return;
			}
			await ctx.Response.WriteAsJsonAsync(result);
		});
		
		app.MapGet("/Movies/Genre/{name}", async (HttpContext ctx, string name) =>
		{
			var result = await service.GetMoviesBy(GetByArg.Genre, name);
			
			if (!result.Any())
			{
				ctx.Response.StatusCode = 404;
				await ctx.Response.WriteAsync("Genre was not found");
				return;
			}
			await ctx.Response.WriteAsJsonAsync(result);
		});
		
		app.MapPost("/Movies", async (HttpContext ctx, MoviePost movie) =>
		{
			var isSaved = await service.SaveMovie(movie);
			
			if (!isSaved)
			{
				await ctx.Response.WriteAsync("The movie is already added");
				return;
			}
			
			await ctx.Response.WriteAsync("The movie is added successfully");
			
		});
		
		
		app.Run();
		
		
		
		// Get
		// /Movies - GetAllMovies
		// /Movies/{name}/{age?} - GetByName + ByAge (optional)
		// /Movies/Actor/{name} - GetAllMoviesWithActor
		// /Movies/Genre/{name} - GetAllMoviesWithGenre
		// /Movies/Genres - GetAllGenres TODO
		// /Movies/Actors - GetAllActors TODO
		
		//Post
		// /Movies - AddMovie (with actors and genres)
		// /
	}
}