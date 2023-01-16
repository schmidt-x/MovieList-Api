using System.Data;
using System.Data.SqlClient;
using Dapper;
using Microsoft.Extensions.Configuration;
using MovieApi.Models;

namespace MovieApi.Services;

public class ActorService : IActorService
{
	private readonly string? _connectionString;
	
	public ActorService(IConfiguration config)
		=> _connectionString = config.GetConnectionString("Default");
		
	private IDbConnection CreateConnection() => new SqlConnection(_connectionString);
	
	public async Task<ActorWrap> GetActorsAsync()
	{
		using var cnn = CreateConnection();
		
		var getActorsSql = "SELECT Id, Name, Info, Link from Actor";
		
		var actors = (await cnn.QueryAsync<Actor>(getActorsSql)).ToList();
		
		var result = new ActorWrap { Actors = actors, Count = actors.Count };
		
		return result;
	}
}