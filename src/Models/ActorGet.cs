namespace MovieApi.Models;

public class ActorGet
{
	public int Count { get; set; }
	public List<ActorIdontKnowYet>? Actors { get; set; }
}

public class ActorIdontKnowYet
{
	public string Name { get; set; } = string.Empty;
	public string Link { get; set; } = string.Empty;
}