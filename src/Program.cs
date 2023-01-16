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
		
		builder.Services.AddHttpContextAccessor(); // for Serilog.Enrichers 
		builder.Services.AddControllers();
		
		builder.Services.AddSingleton<IMovieService, MovieService>();
		builder.Services.AddSingleton<IActorService, ActorService>();
		builder.Services.AddSingleton<IGenreService, GenreService>();
		
		var app = builder.Build();
		
		app.UseSerilogRequestLogging();
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