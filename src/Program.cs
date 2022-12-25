using Microsoft.AspNetCore.Http;
using MovieApi.Models;
using MovieApi.Services;

namespace MovieApi;

public class Program
{
	public static void Main()
	{
		var builder = WebApplication.CreateBuilder();
		
		builder.Services.AddSingleton<IMovieService, MovieService>();
		builder.Services.AddSingleton<IGenreService, GenreService>();
		builder.Services.AddSingleton<IActorService, ActorService>();
		
		var app = builder.Build();
		
		var serviceM = app.Services.GetRequiredService<IMovieService>();
		var serviceG = app.Services.GetRequiredService<IGenreService>();
		var serviceA = app.Services.GetRequiredService<IActorService>();
		
		#region /Movie
		app.MapGet("/Movies", async (ctx) =>
		{
			var result = await serviceM.GetMovies();
			
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
			var result = await serviceM.GetMovies(name, age); // the use of the same method that returns all movies
			
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
			var result = await serviceM.GetMoviesBy(GetByArg.Actor, name);
			
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
			var result = await serviceM.GetMoviesBy(GetByArg.Genre, name);
			
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
			var isSaved = await serviceM.SaveMovie(movie);
			
			if (!isSaved)
			{
				await ctx.Response.WriteAsync("The movie is already added");
				return;
			}
			
			await ctx.Response.WriteAsync("The movie is added successfully");
			
		});
		#endregion
		
		
		app.MapGet("/Genre", async ctx =>
		{
			var result = await serviceG.GetGenresAsync();
			
			if (result.Count == 0)
			{
				await ctx.Response.WriteAsync("No Genres were found");
				return;
			}
			
			await ctx.Response.WriteAsJsonAsync(result);
		});
		
		app.MapGet("/Actor", async ctx =>
		{
			var result = await serviceA.GetActorsAsync();
			
			if (result.Count == 0)
			{
				await ctx.Response.WriteAsync("No Actors were found");
				return;
			}
			
			await ctx.Response.WriteAsJsonAsync(result);
		});
		
		
		app.Run();
		
		
		
		// Get
		// /Movies - GetAllMovies
		// /Movies/{name}/{age?} - GetByName + ByAge (optional)
		// /Movies/Actor/{name} - GetAllMoviesWithActor
		// /Movies/Genre/{name} - GetAllMoviesWithGenre
		//
		// /Genres - GetAllGenres
		// /Actors - GetAllActors
		
		//Post
		// /Movies - AddMovie (with actors and genres)
		// /
	}
}