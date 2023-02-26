namespace MovieApi.DTOs;

public class MoviePut
{
	public string? Title { get; set; }
	public int Release { get; set; }
	public string? Country { get; set; }
	public int Duration { get; set; }
	public string? About { get; set; }
}