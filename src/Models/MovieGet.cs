namespace MovieApi.Models;

public class MovieGet
{
	public int Id { get; set; }
	public string? Title { get; set; }
	public string? Country { get; set; }
	public int Duration { get; set; }
	public int Released { get; set; }
	public List<Actor>? Actors { get; set; }
	public List<string>? Genres { get; set; }
}

public class Actor
{
	public int Id { get; set; }
	public string? Name { get; set; }
	public string? Info { get; set; }
}

public class GetMoviesBy
{
	public string Title { get; set; } = string.Empty;
	public string Link { get; set; } = string.Empty; 
}