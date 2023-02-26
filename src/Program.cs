using MovieApi.Middleware;
using MovieApi.Repositories;
using MovieApi.Services;
using Serilog;

namespace MovieApi;

public class Program
{
	public static void Main()
	{
		var builder = WebApplication.CreateBuilder();
		
		builder.Host.UseSerilog((_, config) =>
			config.ReadFrom.Configuration(builder.Configuration));
		builder.Services.AddCors(options =>
		{
			options.AddPolicy("MyPolicy", policy =>
			{
				policy.WithOrigins("http://localhost:63342");
			});
		});
		builder.Services.AddControllers();
		builder.Services.AddHttpContextAccessor();
		builder.Services.AddSingleton<IMovieService, MovieService>();
		builder.Services.AddSingleton<IMovieRepository, MovieRepository>();
		builder.Services.AddSingleton<IActorService, ActorService>();
		// builder.Services.AddSingleton<IGenreService, GenreService>();
		builder.Services.AddSingleton<IMemberRepository, MemberRepository>();
		builder.Services.AddSingleton<ActionService>();
		
		var app = builder.Build();
		app.UseCors("MyPolicy");
		app.UseSerilogRequestLogging();
		app.UseExceptionHandlerMiddleware();
		app.MapControllers();
		
		try
		{
			Log.Information("App is starting");
			app.Run();
		}
		catch(Exception e)
		{
			Log.Fatal(e, "Error has occured trying to start the application");
		}
		finally
		{
			Log.CloseAndFlush();
		}
	}
}

// api/movies/..
// api/movies/byactor/...
// api/movies/bygenre/...
// api/actors/...
// api/genres/...