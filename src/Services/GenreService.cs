using System.Data.SqlClient;
using Dapper;
using Microsoft.Extensions.Configuration;
using MovieApi.Models;

namespace MovieApi.Services;

public class GenreService : IGenreService
{
	private readonly string _connectionString;
	
	public GenreService(IConfiguration config)
		=> _connectionString = config.GetConnectionString("Default");
	
	public async Task<GenreGet> GetGenresAsync()
	{
		using var cnn = new SqlConnection(_connectionString);
		
		var getGenresSql = 
			@"SELECT Type FROM Genre";
		
		var genres = (await cnn.QueryAsync<Genre>(getGenresSql)).ToList();
		
		foreach(var genre in genres)
			genre.Link = $"https://localhost:7777/Movies/Genre/{genre.Type}";
		
		// var result = new GenreGet() { Genre = res, Count = res.Count };
		
		return new GenreGet() { Genres = genres, Count = genres.Count };
	}
}
/*
public async Task<Genre


*/