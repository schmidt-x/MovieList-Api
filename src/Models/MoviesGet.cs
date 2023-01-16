namespace MovieApi.Models;

public class MovieWrap
{
	public int Count { get; set; }
	public IEnumerable<MoviesGet>? Movies { get; set; }
}

public class MovieGet
{
	public int Id { get; set; }
	public string? Title { get; set; }
	public string? Country { get; set; }
	public int Duration { get; set; }
	public int Released { get; set; }
	public string? About { get; set; }
	public List<Actor>? Actors { get; set; }
	public List<Genre>? Genres { get; set; }
}
public class MoviesGet
{
	public int Id { get; set; }
	public string Title { get; set; } = String.Empty;
	public int Duration { get; set; }
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
	public int Id { get; set; }
	public string Type { get; set; } = String.Empty;
	public string? Link { get; set; }
}

public class ActorWrap
{
	public int Count { get; set; }
	public IEnumerable<Actor>? Actors { get; set; }
}
public class GenreWrap
{
	public int Count { get; set; }
	public IEnumerable<Genre>? Genres { get; set; }
}

public enum GetBy
{
	Actor = 1,
	Genre
}