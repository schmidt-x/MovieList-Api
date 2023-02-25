using System.Data;
using System.Data.SqlClient;
using Dapper;
using Dapper.Transaction;
using Microsoft.Extensions.Configuration;
using MovieApi.DTOs;
using MovieApi.SqlQueries;

namespace MovieApi.Repository;

public class MovieRepository : IMovieRepository
{
	private readonly string? _connectionString;
	private IDbConnection CreateConnection() => new SqlConnection(_connectionString);
	public MovieRepository(IConfiguration config)
	{
		_connectionString = config.GetConnectionString("Default");
	}
	
	public async Task<MovieGet?> QueryGetAsync(int id)
	{
		using var cnn = CreateConnection();
		
		var unsorted = await cnn.QueryAsync<MovieGet, ActorGet, GenreGet, MovieGet>(MovieSql.Get, (movie, actor, genre) =>
		{
			if (actor != null)
				movie.Actors = new List<ActorGet> { actor };
			if (genre != null)
				movie.Genres = new List<GenreGet> { genre };
			
			return movie;
		}, new { movieId = id }, splitOn: "Id, Id");
		
		var result = unsorted.GroupBy(g => g.Id).Select(g =>
		{
			var movie = g.First();
			
			if (movie.Actors != null)
			{
				movie.Actors = g
					.Select(s => s.Actors!.First())
					.DistinctBy(k => k.Id)
					.ToList();
			}
				
			if (movie.Genres != null)
			{
				movie.Genres = g
					.Select(s => s.Genres!.First())
					.DistinctBy(k => k.Id)
					.ToList();
			}
		
			return movie;
				
		}).FirstOrDefault();
			
		return result;
	}
	public async Task<Wrap<MoviesGet>> QueryGetAllAsync(string sql, int? from, int? to, int? byId = null)
	{
		using var cnn = CreateConnection();
		var movies = (await cnn.QueryAsync<MoviesGet>(sql, new { from, to, byId })).ToList();
		
		return new Wrap<MoviesGet> { Count = movies.Count, List = movies };
	}
	public async Task<int> QuerySaveAsync(MoviePost movie, int[] actorId, int[] genreId)
	{
		int movieId;
		
		using var cnn = CreateConnection();
		cnn.Open();
		using var transaction = cnn.BeginTransaction();
		
		try 
		{
			using var multi = await transaction.QueryMultipleAsync(MovieSql.Save, movie);
			var isDuplicate = multi.ReadFirst<bool>();
			movieId = multi.ReadFirst<int>();
			
			if (isDuplicate) return movieId * -1;
			
			foreach(var id in actorId)
			{
				var isAttached = await transaction.ExecuteAsync(ActorSql.Attach, new { actorId = id, movieId }) > 0;
				if (isAttached) continue;
				throw new ArgumentException($"Actor not valid ({id})", nameof(actorId));
			}
			
			foreach(var id in genreId)
			{
				var isAttached = await transaction.ExecuteAsync(GenreSql.Attach, new { genreid = id, movieId }) > 0;
				if (isAttached) continue;
				throw new ArgumentException($"Genre not valid ({id})", nameof(genreId));
			}
			
			foreach(var actor in movie.Actors!)
				await transaction.ExecuteAsync(ActorSql.SaveAndAttach, new { actor.Name, actor.Info, movieId });
			
			foreach(var genre in movie.Genres!)
				await transaction.ExecuteAsync(GenreSql.SaveAndAttach, new { genre.Type, movieId });
			
			transaction.Commit();
		}
		catch
		{
			transaction.Rollback();
			throw;
		}
		
		return movieId;
	}
	public async Task QueryDeleteAsync(int[] movieId)
	{
		using var cnn = CreateConnection();
		int deleted = 0; 
		
		foreach(var id in movieId)
			if (await cnn.ExecuteAsync(MovieSql.Delete, new { movieId = id }) > 0)
				deleted++;
		
		if (deleted <= 0)
			throw new ArgumentException("Movie(s) not valid", nameof(movieId));
	}
	public async Task<MovieGet> QueryUpdateAsync(MoviePost movie, int[] actorIdDel, int[] genreIdDel, int[] actorIdAdd, int[] genreIdAdd)
	{
		
		
		
		throw new NotImplementedException();
	}
}