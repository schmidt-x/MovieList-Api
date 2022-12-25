using System.Data.SqlClient;
using Dapper;
using Microsoft.Extensions.Configuration;
using MovieApi.Models;

namespace MovieApi.Services;

public class ActorService : IActorService
{
	private readonly string _connectionString;
	
	public ActorService(IConfiguration config)
		=> _connectionString = config.GetConnectionString("Default");
	
	public async Task<ActorGet> GetActorsAsync()
	{
		using var cnn = new SqlConnection(_connectionString);
		
		var getActorsSql =
			@"SELECT Name from Actor";
		
		var actors = (await cnn.QueryAsync<ActorIdontKnowYet>(getActorsSql)).ToList(); // don't forget to check it with an empty return
		
		foreach(var actor in actors)
			actor.Link = $"https://localhost:7777/Movies/Actor/{actor.Name.Replace(' ', '_')}";
		
		var result = new ActorGet { Actors = actors, Count = actors.Count };
		
		return result;
	}
}