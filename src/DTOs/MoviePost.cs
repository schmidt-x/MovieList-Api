namespace MovieApi.DTOs;

public class MoviePost
{
	public string? Title { get; set; }
	public string? Country { get; set; }
	public int Duration { get; set; }
	public int Released { get; set; }
	public string? About { get; set; }
	public IEnumerable<ActorGet>? Actors { get; set; }
	public IEnumerable<GenreGet>? Genres { get; set; }
}

