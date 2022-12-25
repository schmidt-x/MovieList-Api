namespace MovieApi;

public class Program
{
	public static void Main()
	{
		var builder = WebApplication.CreateBuilder();
		
		var app = builder.Build();
		
		app.Run();
	}
}