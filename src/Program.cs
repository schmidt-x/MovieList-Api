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
		// /Movie(?unfolded=bool&from=int&to=int&sortBy=string,desc=true) = return all the movies 
		//		unfolded = in an unfolded form
		//		from - to = filter the movies by released date
		//		sortBy=title,released,duration = return sorted
		//		+ desc = as optional to sortBy
		
		// /Movies/{title}/{released?} = return a single movie by Title + Released date as optional
		
		// /Movies/Actor/{name}(orderBy=string, desc=true) = return all the movies with an Actor
		
		// /Movies/Genre/{type}(orderBy=string, desc=true) = return all the movies with a type of a Genre
		
		// 
		// 
		// 
		
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