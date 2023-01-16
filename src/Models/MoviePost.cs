namespace MovieApi.Models;

public class MoviePost
{
	public string Title { get; set; } = String.Empty;
	public string Country { get; set; } = String.Empty;
	public int Duration { get; set; }
	public int Released { get; set; }
	public List<Actor> Actors { get; set; } = new();
	public List<Genre> Genres { get; set; } = new();
}