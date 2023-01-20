using System.Data;
using System.Data.SqlClient;
using Dapper;
using Dapper.Transaction;
using Microsoft.Extensions.Configuration;
using MovieApi.DTOs;
using MovieApi.SqlQueries;
using Serilog;

namespace MovieApi.Services;

public class MovieService : IMovieService
{
	private readonly string? _connectionString;
	private readonly Dictionary<string, string> _orderBy;
	private IDbConnection CreateConnection() => new SqlConnection(_connectionString);
	public MovieService(IConfiguration config)
	{
		_connectionString = config.GetConnectionString("Default");
		_orderBy = new()
		{
			{"title", " ORDER BY Movie.Title"},
			{"released", " ORDER BY Movie.Released"},
			{"duration", " ORDER BY Movie.Duration"}
		};
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
			Log.Error(ex, "Something went wrong");
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
	public Task<string> SaveAsync(MoviePost movie)
	{
		// probably I'll do something here before saving the movie in future
		
		return SaveMovieOnTransactAsync(movie);
	}
	public async Task<bool> DeleteAsync(int id)
	{
		try
		{
			using var cnn = CreateConnection();
			var isDeleted = await cnn.ExecuteAsync(MovieSql.Delete, new { movieId = id }) != 0;
			Log.Information(isDeleted ? "Movie deleted" : "Couldn't find the movie");
			return isDeleted;
		}
		catch(Exception ex)
		{
			Log.Error(ex, "Something went wrong");
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
			Log.Information("Movies are received from db");
			return new Wrap<MoviesGet> { Count = movies.Count, List = movies };
		}
		catch(Exception ex)
		{
			Log.Error(ex, "Something went wrong");
			throw;
		}
	}
	private async Task<string> SaveMovieOnTransactAsync(MoviePost movie)
	{
		string movieLink;
		try
		{
			using var cnn = CreateConnection();
			cnn.Open();
			using var transaction = cnn.BeginTransaction();
			
			using var multi = await transaction.QueryMultipleAsync(MovieSql.Save, movie);
			var isMatched = await multi.ReadFirstAsync<bool>();
			var movieId = await multi.ReadFirstAsync<int>();
			movieLink = $"https://localhost:7777/Movie/{movieId}";
			
			if (isMatched)
			{
				Log.Information("Movie duplicate");
				return "This movie has already been added before\n" + movieLink;
			}
				
			foreach(var actor in movie.Actors!)
				await transaction.ExecuteAsync(ActorSql.Save, new { actor.Name, actor.Info, movieId });
				
			foreach(var genre in movie.Genres!)
				await transaction.ExecuteAsync(GenreSql.Save, new { genre.Type, movieId });
			
			transaction.Commit();
			Log.Information("Movie is saved into db"); 
		}
		catch(Exception ex)
		{
			Log.Error(ex, "Something went wrong");
			throw;
		}
		
		return "The movie is saved successfully\n" + movieLink;
	}
}