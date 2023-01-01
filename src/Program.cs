using Microsoft.AspNetCore.Http;
using MovieApi.Models;
using MovieApi.Services;
using Serilog;

namespace MovieApi;

public class Program
{
	public static void Main()
	{
		var builder = WebApplication.CreateBuilder();
		
		builder.Host.UseSerilog((context, config) =>
		{
			config.ReadFrom.Configuration(builder.Configuration);
		});
		
		builder.Services.AddHttpContextAccessor(); // for Serilog.Enrichers 
		builder.Services.AddControllers();
		
		builder.Services.AddSingleton<IMovieService, MovieService>();
		builder.Services.AddSingleton<IGenreService, GenreService>();
		builder.Services.AddSingleton<IActorService, ActorService>();
		
		var app = builder.Build();
		
		app.UseSerilogRequestLogging();
		app.MapControllers();
		
		var serviceM = app.Services.GetRequiredService<IMovieService>();
		var serviceG = app.Services.GetRequiredService<IGenreService>();
		var serviceA = app.Services.GetRequiredService<IActorService>();
		
		#region /Movie
		// app.MapGet("/Movie", async (HttpContext ctx, int? fromDate, int? toDate) => // TODO
		// {
		// 	var result = await serviceM.GetMovies();
		// 	
		// 	if (result.Count == 0)
		// 	{
		// 		ctx.Response.StatusCode = 404;
		// 		await ctx.Response.WriteAsync("Movies were not found");
		// 		return;
		// 	}
		// 	await ctx.Response.WriteAsJsonAsync(result);
		// });
		
		// app.MapGet("/Movie/{name}/{age:int?}", async (HttpContext ctx, string name, int? age) =>
		// {
		// 	var result = await serviceM.GetMovies(name, age); // the use of the same method that returns all movies
		// 	
		// 	if (result.Count == 0)
		// 	{
		// 		ctx.Response.StatusCode = 404;
		// 		await ctx.Response.WriteAsync("Movie was not found");
		// 		return;
		// 	}
		// 	await ctx.Response.WriteAsJsonAsync(result.Movies!.First());
		// });
		
		app.MapGet("/Movie/Actor/{name}", async (HttpContext ctx, string name) =>
		{
			var result = await serviceM.GetMoviesBy(GetBy.Actor, name);
			
			if (result.Count == 0)
			{
				ctx.Response.StatusCode = 404;
				await ctx.Response.WriteAsync("Actor or movies with specified actor were not found");
				return;
			}
			await ctx.Response.WriteAsJsonAsync(result);
		});
		
		app.MapGet("/Movie/Genre/{name}", async (HttpContext ctx, string name) =>
		{
			var result = await serviceM.GetMoviesBy(GetBy.Genre, name);
			
			if (result.Count == 0)
			{
				ctx.Response.StatusCode = 404;
				await ctx.Response.WriteAsync("Genre or movies with specified genre were not found");
				return;
			}
			await ctx.Response.WriteAsJsonAsync(result);
		});
		
		app.MapPost("/Movie", async (HttpContext ctx, MoviePost movie) =>
		{
			var success = await serviceM.SaveMovie(movie);
			
			if (!success)
			{
				await ctx.Response.WriteAsync("The movie is already added");
				return;
			}
			
			var link = GetLink(movie);
			await ctx.Response.WriteAsync(link);
			
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
		
		
		try
		{
			Log.Information("App is starting {time:hh:mm:ss}", DateTime.Now);
			app.Run();
		}
		catch(Exception e)
		{
			Log.Fatal(e, "Error has occured trying to start the application {time:hh:mm:ss}", DateTime.Now);
			Log.Warning("App is closed");
		}
		finally
		{
			Log.CloseAndFlush();
		}
		
		
		
		// Get
		// /Movies - GetAllMovies //TODO replace Movies to Movie
		// /Movies/{name}/{age?} - GetByName + ByAge (optional)
		// /Movies/Actor/{name} - GetAllMoviesWithActor
		// /Movies/Genre/{name} - GetAllMoviesWithGenre
		//
		// /Genre - GetAllGenres
		// /Actor - GetAllActors
		
		//Post
		// /Movies - AddMovie (with actors and genres)
		// /
		
		// TODO create DapperContext class and inject it   
		// done
		// added method that creates a sqlConnection
	}
	private static string GetLink(MoviePost movie)
	{
		return 
			"Movie added successfully\n" +
			$"https://localhost:7777/Movie/{movie.Title.Replace(' ', '_')}/{movie.Released}";
	}
}