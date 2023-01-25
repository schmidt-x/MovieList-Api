using System.Data;
using System.Data.SqlClient;
using Dapper;
using Dapper.Transaction;
using Microsoft.Extensions.Configuration;
using MovieApi.DTOs;
using MovieApi.SqlQueries;
using Microsoft.Extensions.Logging;

namespace MovieApi.Services;

public class MovieService : IMovieService
{
	private readonly string? _connectionString;
	private readonly ILogger<MovieService> _logger;
	private readonly Dictionary<string, string> _orderBy;
	private IDbConnection CreateConnection() => new SqlConnection(_connectionString);
	public MovieService(IConfiguration config, ILogger<MovieService> logger)
	{
		_connectionString = config.GetConnectionString("Default");
		_orderBy = new()
		{
			{"title", " ORDER BY Movie.Title"},
			{"released", " ORDER BY Movie.Released"},
			{"duration", " ORDER BY Movie.Duration"}
		};
		_logger = logger;
	}
	
	
	public async Task<MovieGet?> GetByIdAsync(int id)
	{
		try
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
		catch(Exception ex)
		{
			_logger.LogError(ex, "Failed to get the movie");
			throw;
		}
	}
	public Task<Wrap<MoviesGet>> GetAllAsync(int? from, int? to, string? orderBy, bool desc)
	{
		var sql = MovieSql.GetAll;
		
		if (from != null || to != null)
			sql += AddConditions(from, to, false);
			
		if (orderBy != null && _orderBy.ContainsKey(orderBy))
			sql += AddOrdering(orderBy, desc);
		
		return GetAllAsync(sql, from, to);
	}
	public Task<Wrap<MoviesGet>> GetAllByAsync(int id, GetBy arg, int? from, int? to, string? orderBy, bool desc)
	{
		string sql = arg switch
		{
			GetBy.Actor => MovieSql.GetAllByActor,
			GetBy.Genre => MovieSql.GetAllByGenre,
			_ => throw new ArgumentException("there is no way it can get here")
		};
		
		if (from != null || to != null)
			sql += AddConditions(from, to, true);
		
		if (orderBy != null && _orderBy.ContainsKey(orderBy))
			sql += AddOrdering(orderBy, desc);
		
		return GetAllAsync(sql, from, to, id);
	}
	public Task<string> SaveAsync(MoviePost movie, int[] actorId, int[] genreId)
	{
		// probably in future I'll do something here before saving the movie
		
		return SaveMovieOnTransactAsync(movie, actorId, genreId);
	}
	public async Task<int> DeleteAsync(int[] ids)
	{
		try
		{
			using var cnn = CreateConnection();
			
			// number of movies deleted
			int deleted = 0; 
			
			foreach(var id in ids)
			{
				if (await cnn.ExecuteAsync(MovieSql.Delete, new { movieId = id }) > 0)
					deleted++;
			}
			
			return deleted;
		}
		catch(Exception ex)
		{
			_logger.LogError(ex, "Failed to delete the movies");
			throw;
		}
	}
	
	private string AddOrdering(string orderBy, bool desc)
	{
		string sql = _orderBy[orderBy];
		if (desc) sql += " DESC";
		
		return sql;
	}
	private string AddConditions(int? from, int? to, bool hasCondition)
	{
		string sql = hasCondition
			? " AND"
			: " WHERE";
		
		if (from != null)
		{
			sql += " Movie.Released >= @from";
			if (to != null)
				sql += " AND Movie.Released <= @to";
		}
		else
			sql += " Movie.Released <= @to";
		
		return sql;
	}
	private async Task<Wrap<MoviesGet>> GetAllAsync(string sql, int? from, int? to, int? byId = null)
	{
		try
		{
			using var cnn = CreateConnection();
			var movies = (await cnn.QueryAsync<MoviesGet>(sql, new { from, to, byId })).ToList();
			return new Wrap<MoviesGet> { Count = movies.Count, List = movies };
		}
		catch(Exception ex)
		{
			_logger.LogError(ex, "Failed to get the movies");
			throw;
		}
	}
	private async Task<string> SaveMovieOnTransactAsync(MoviePost movie, int[] actorId, int[] genreId)
	{
		bool isRolledBack = false;
		string movieLink;
		try
		{
			using var cnn = CreateConnection();
			cnn.Open();
			using var transaction = cnn.BeginTransaction();
			try
			{
				using var multi = await transaction.QueryMultipleAsync(MovieSql.Save, movie);
				var isMatched = multi.ReadFirst<bool>();
				var movieId = multi.ReadFirst<int>();
				movieLink = $"https://localhost:7001/Movie/{movieId}";
				
				if (isMatched)
					return "The movie already exists\n" + movieLink;
				
				foreach(var id in actorId)
				{
					var isAttached = await transaction.ExecuteAsync(ActorSql.Attach, new { actorId = id, movieId }) > 0;
					if (isAttached) continue;
					throw new ArgumentException($"Actor '{id}' is not valid", nameof(actorId));
				}
				
				foreach(var id in genreId)
				{
					var isAttached = await transaction.ExecuteAsync(GenreSql.Attach, new { genreid = id, movieId }) > 0;
					if (isAttached) continue;
					throw new ArgumentException($"Genre '{id}' is not valid", nameof(genreId));
				}
				
				foreach(var actor in movie.Actors!)
					await transaction.ExecuteAsync(ActorSql.SaveAndAttach, new { actor.Name, actor.Info, movieId });
				
				foreach(var genre in movie.Genres!)
					await transaction.ExecuteAsync(GenreSql.SaveAndAttach, new { genre.Type, movieId });
				
				transaction.Commit();
			}
			catch(Exception ex)
			{
				transaction.Rollback();
				isRolledBack = true;
				_logger.LogError(ex, "Transaction failed");
				throw;
			}
		}
		catch(Exception ex)
		{
			if (!isRolledBack)
				_logger.LogError(ex, "Failed to open connection/begin transaction");
			throw;
		}
		
		return "The movie is saved successfully\n" + movieLink;
	}
}