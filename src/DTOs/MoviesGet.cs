namespace MovieApi.DTOs;

public class MoviesGet
{
	public int Id { get; set; }
	public string? Title { get; set; }
	public int Duration { get; set; }
	public int Release { get; set; }
}