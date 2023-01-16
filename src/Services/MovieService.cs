using System.Data;
using System.Data.SqlClient;
using Dapper;
using Dapper.Transaction;
using Microsoft.Extensions.Configuration;
using MovieApi.Models;
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
		
			var unsorted = await cnn.QueryAsync<MovieGet, Actor, Genre, MovieGet>(MovieSql.GetById, (movie, actor, genre) =>
			{
				if (actor != null)
					movie.Actors = new() { actor };
				if (genre != null)
					movie.Genres = new() { genre };
				
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
		catch(Exception e)
		{
			Log.Error(e, "Something went wrong");
			throw;
		}
	} 
	public Task<MovieWrap> GetAllAsync(int? from, int? to, string? orderBy, bool desc)
	{
		var sql = MovieSql.GetAll;
		
		if (from != null || to != null)
			sql += AddConditions(from, to, false);
			
		if (orderBy != null && _orderBy.ContainsKey(orderBy))
			sql += AddOrdering(orderBy, desc);
		
		return GetAllAsync(sql, from, to);
	}
	public Task<MovieWrap> GetAllByAsync(int id, GetBy arg, int? from, int? to, string? orderBy, bool desc)
	{
		string sql = arg switch
		{
			GetBy.Actor => MovieSql.ByActor,
			GetBy.Genre => MovieSql.ByGenre,
			_ => throw new Exception()
		};
		
		if (from != null || to != null)
			sql += AddConditions(from, to, true);
		
		if (orderBy != null && _orderBy.ContainsKey(orderBy))
			sql += AddOrdering(orderBy, desc);
		
		return GetAllAsync(sql, from, to, id);
	}
	public Task<string> SaveMovie(MoviePost movie)
	{
		foreach(var actor in movie.Actors)
		{
			if (actor.Info != "") continue;
			var replaced = actor.Name.Replace(' ', '_');
			actor.Info = $"https://en.wikipedia.org/wiki/{replaced}";
		}
		
		return SaveMovieOnTransactAsync(movie);
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
	private async Task<MovieWrap> GetAllAsync(string sql, int? from, int? to, int? byId = null)
	{
		using var cnn = CreateConnection();
		
		var movies = (await cnn.QueryAsync<MoviesGet>(sql, new { from, to, byId })).ToList();
		
		return new MovieWrap { Count = movies.Count, Movies = movies };
	}
	private async Task<string> SaveMovieOnTransactAsync(MoviePost movie)
	{
		string movieLink;
		try
		{
			using var cnn = CreateConnection();
			cnn.Open();
			using var transaction = cnn.BeginTransaction();
			
			using var multi = await transaction.QueryMultipleAsync(MovieSql.SaveMovie, movie);
			var matched = await multi.ReadFirstAsync<bool>();
			var movieId = await multi.ReadFirstAsync<int>();
			movieLink = $"https://localhost:7777/Movie/{movieId}";
			
			if (matched)
			{
				Log.Information("Movie duplicate");
				return "This movie is added before\n" + movieLink;
			}
			
			foreach(var actor in movie.Actors)
				await transaction.ExecuteAsync(MovieSql.SaveActors, new { actor.Name, actor.Info, movieId });
			
			foreach(var genre in movie.Genres)
				await transaction.ExecuteAsync(MovieSql.SaveGenres, new { genre.Type, movieId });
			transaction.Commit();
		}
		catch(Exception e)
		{
			Log.Error(e, "something went wrong");
			throw;
		}
		
		Log.Information("Movie is saved"); 
		return "The movie is saved successfully\n" + movieLink;
	}
	
}