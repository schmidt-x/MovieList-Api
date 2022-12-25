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
		
		var getGenresSql = "SELECT Type, Link FROM Genre";
		
		var genres = (await cnn.QueryAsync<Genre>(getGenresSql)).ToList();
		
		var result = new GenreGet { Genres = genres, Count = genres.Count };
		
		return result;
	}
}