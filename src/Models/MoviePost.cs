namespace MovieApi.Models;

public class MoviePost
{
	public string? Title { get; set; }
	public string? Country { get; set; }
	public int Duration { get; set; }
	public int Released { get; set; }
	public List<Actors> Actors { get; set; } = new ();
	public List<Genres> Genres { get; set; } = new ();
}

public class Actors
{
	public string Name { get; set; } = string.Empty;
	public string Info { get; set; } = string.Empty;
}

public class Genres
{
	public string? Type { get; set; }
}


public class ActorIds
{
	public int Id { get; set; }
}

public class GenreIds
{
	public int Id { get; init; }
}
