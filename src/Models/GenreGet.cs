namespace MovieApi.Models;

public class GenreGet
{
	public int Count { get; set; }
	public List<Genre>? Genres { get; set; }
}

public class Genre
{
	public string? Type { get; set; }
	public string? Link { get; set; }
}
