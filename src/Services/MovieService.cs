using System.Data;
using System.Data.SqlClient;
using Dapper;
using Dapper.Transaction;
using Microsoft.Extensions.Configuration;
using MovieApi.Models;
using MovieApi.SqlQueries;

namespace MovieApi.Services;

public class MovieService : IMovieService
{
	private readonly string _connectionString;
	private readonly Dictionary<string, string> _orderByDic;
	
	public MovieService(IConfiguration config)
	{
		_connectionString = config.GetConnectionString("Default");
		_orderByDic = new()
		{
			{"title", " ORDER BY Movie.Title"},
			{"released", " ORDER BY Movie.Released"},
			{"duration", " ORDER BY Movie.Duration"}
		};
	} 
		
	private IDbConnection CreateConnection() => new SqlConnection(_connectionString);
	


	public async Task<MovieGetUnfolded?> GetSingleAsync(string title, int? released)
	{
		var sql = MovieSql.GetMoviesUnfolded;
		
		title = title.Replace('_', ' ');
		sql += " where Movie.Title = @title";
		
		if (released != null)
			sql += " and Movie.Released = @released";
		
		var result = await GetAllUnfoldedAsync(sql, title, released);
		
		return result.Movies.FirstOrDefault() as MovieGetUnfolded;
	}
	
	
	public Task<MovieWrap> GetAllAsync(int? from, int? to, bool unfolded, string? orderBy, bool desc)
	{
		var sql = unfolded
			? MovieSql.GetMoviesUnfolded
			: MovieSql.GetMovies;
		
		
		if (from != null || to != null)
			sql += AddConditions(from, to);
			
		if (orderBy != null && _orderByDic.ContainsKey(orderBy))
			sql += AddOrdering(orderBy, desc);
		
		return unfolded
			? GetAllUnfoldedAsync(sql, from: from, to: to)
			: GetAllAsync(sql, from, to);
	}
	
	private string AddOrdering(string orderBy, bool desc)
	{
		string sql = _orderByDic[orderBy];
		if (desc) sql += " DESC";
		
		return sql;
	}
	private string AddConditions(int? from, int? to)
	{
		string sql;
		if (from != null)
		{
			sql = " where Movie.Released >= @from";
			if (to != null)
				sql += " and Movie.Released <= @to";
		}
		else
			sql = " where Movie.Released <= @to";
		return sql;
	}
	
	private async Task<MovieWrap> GetAllUnfoldedAsync
		(string sql, string? title = null, int? released = null, int? from = null, int? to = null)
	{
		using var cnn = CreateConnection();
		
		var movies = await cnn.QueryAsync<MovieGetUnfolded, Actor, Genre, MovieGetUnfolded>(sql, (movie, actor, genre) =>
		{
			if (actor != null)
				movie.Actors = new() { actor };
			if (genre != null)
				movie.Genres = new() { genre };
			
			return movie;
		}, new { title, released, from, to } , splitOn: "Id, Type");
		
		var result = movies.GroupBy(m => m.Id).Select(g =>
		{
			var movie = g.First();
			
			try
			{
				movie.Actors = g
					.Select(m => m.Actors!.Single())
					.DistinctBy(a => a.Id)
					.ToList();
			}
			catch(Exception)
			{
				movie.Actors = null;
			}
			
			try
			{
				movie.Genres = g
					.Select(m => m.Genres!.Single())
					.DistinctBy(a => a.Type) // TODO someday add by Id
					.ToList();
			}
			catch(Exception)
			{
				movie.Genres = null;
			}
			
			return movie;
			
		}).ToList();
		
		return new MovieWrap { Count = result.Count, Movies = result }; // covariance baby)
	}
	
	private async Task<MovieWrap> GetAllAsync(string sql, int? from = null, int? to = null, string? byArg = null)
	{
		using var cnn = CreateConnection();
		
		var movies = (await cnn.QueryAsync<MovieGet>(sql, new { from, to, byArg })).ToList(); // TODO twice toList??? 
		
		return new MovieWrap { Count = movies.Count, Movies = movies };
	} 
	
	public Task<MovieWrap> GetAllByAsync(GetBy arg, string byArg, string? orderBy, bool desc)
	{
		string sql;
		
		switch (arg)
		{
			case GetBy.Actor:
				byArg = byArg.Replace('_', ' ');
				sql = MovieSql.ByActor;
				break;
			case GetBy.Genre:
				sql = MovieSql.ByGenre;
				break;
			case GetBy.Country:
				throw new NotImplementedException();
			default:
				throw new ArgumentException("there is no way it can reach here");
		}
		
		if (orderBy != null && _orderByDic.ContainsKey(orderBy))
			sql += AddOrdering(orderBy, desc);
		
		return GetAllAsync(sql, byArg: byArg);
	}
	
	
	public Task<bool> SaveMovie(MoviePost movie) // json from the body
	{
		List<Actor> actors = movie.Actors;
		List<Genre> genres = movie.Genres;
		
		// adding some basic Info about actors
		foreach(var actor in actors)
		{
			if (actor.Info != "") continue;
			
			var replaced = actor.Name.Replace(' ', '_');
			actor.Info = $"https://en.wikipedia.org/wiki/{replaced}";
		}
		
		return SaveMovie(movie, actors, genres);
	}
	
	private Task<bool> SaveMovie(MoviePost movie, List<Actor> actors, List<Genre> genres)
	{
		try
		{
			using var cnn = CreateConnection();
			cnn.Open();
			using var transaction = cnn.BeginTransaction();
			
			var movieId = transaction.QuerySingleOrDefault<int>(MovieSql.SaveMovie, movie);
			if (movieId == 0) return Task.FromResult(false);
			
			foreach(var actor in actors)
				transaction.Execute(MovieSql.SaveActors, new { actor.Name, actor.Info, movieId });
			
			foreach(var genre in genres)
				transaction.Execute(MovieSql.SaveGenres, new { genre.Type, movieId });
				
			transaction.Commit();
		}
		catch(Exception)
		{
			// Log.Error ..
			return Task.FromResult(false);
		}
		
		// Log.Information ..
		return Task.FromResult(true);
	}
	
}

public enum GetBy
{
	Actor = 1,
	Genre,
	Country // TODO not implemented yet
}