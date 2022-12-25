namespace MovieApi.Models;

public class MoviePost
{
	public string Title { get; set; } = String.Empty;
	public string Country { get; set; } = String.Empty;
	public int Duration { get; set; }
	public int Released { get; set; }
	public string? Link { get; set; }
	public List<Actor> Actors { get; set; } = new ();
	public List<Genre> Genres { get; set; } = new ();
}

public class ActorIds
{
	public int Id { get; set; }
}

public class GenreIds
{
	public int Id { get; init; }
}
