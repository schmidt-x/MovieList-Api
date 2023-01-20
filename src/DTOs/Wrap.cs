namespace MovieApi.DTOs;

public class Wrap<T>
{
	public int Count { get; set; }
	public IEnumerable<T>? List { get; set; }
}