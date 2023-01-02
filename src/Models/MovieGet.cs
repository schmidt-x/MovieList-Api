namespace MovieApi.Models;

public class MovieWrap
{
	public int Count { get; set; }
	public IEnumerable<object> Movies { get; set; }
}

public class MovieGetUnfolded
{
	public int Id { get; set; }
	public string? Title { get; set; }
	public string? Country { get; set; }
	public int Duration { get; set; }
	public int Released { get; set; }
	public List<Actor>? Actors { get; set; }
	public List<Genre>? Genres { get; set; }
}
public class MovieGet
{
	public string Title { get; set; } = String.Empty;
	public int Released { get; set; }
	public string Link { get; set; } = String.Empty;
}

public class Actor
{
	public int Id { get; set; }
	public string Name { get; set; } = String.Empty;
	public string Info { get; set; } = String.Empty;
	public string Link { get; set; } = String.Empty;
}
public class Genre
{
	public string Type { get; set; } = String.Empty;
	public string? Link { get; set; }
}


public class ActorGet
{
	public int Count { get; set; }
	public List<Actor>? Actors { get; set; }
}

public class GenreGet
{
	public int Count { get; set; }
	public List<Genre>? Genres { get; set; }
}