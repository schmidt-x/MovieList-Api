using System.Data;
using System.Data.SqlClient;
using System.Threading;
using Dapper;
using Dapper.Transaction;
using Microsoft.Extensions.Configuration;
using MovieApi.Models;
using MovieApi.SqlQueries;

namespace MovieApi.Services;

public class MovieService : IMovieService
{
	private readonly string _connectionString;
	
	public MovieService(IConfiguration config) => 
		_connectionString = config.GetConnectionString("Default");
		
	private IDbConnection CreateConnection() => new SqlConnection(_connectionString);
	
	
	
	public Task<MovieWrap> GetSingleAsync(string title, int? released = null)
	{
		var sql = MovieSql.GetMovies;
		
		title = title.Replace('_', ' ');
		sql += " where Movie.Title = @title";
		
		if (released != null)
			sql += " and Movie.Released = @released";
		
		return GetAllAsync(sql, title, released);
	}
	
	
	public Task<MovieWrap> GetAllAsync(int? from, int? to, bool folded)
	{
		var sql = folded
			? MovieSql.GetMoviesFolded
			: MovieSql.GetMovies;
		
		if (from != null || to != null)
			sql += GetConditionSql(from, to);
			
		return folded
			? GetAllFoldedAsync(sql, from, to)
			: GetAllAsync(sql, from: from, to: to);
	}
	
	private string GetConditionSql(int? from, int? to)
	{
		string sql = "";
		if (from != null)
		{
			sql += " where Movie.Released >= @from";
			if (to != null)
				sql += " and Movie.Released <= @to";
		}
		else
			sql += " where Movie.Released <= @to";
		return sql;
	}
	
	private async Task<MovieWrap> GetAllAsync(string sql, string? title = null, int? released = null, int? from = null, int? to = null)
	{
		using var cnn = CreateConnection();
		
		var movies = await cnn.QueryAsync<MovieGet, Actor, Genre, MovieGet>(sql, (movie, actor, genre) =>
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
	
	private async Task<MovieWrap> GetAllFoldedAsync(string sql, int? from = null, int? to = null)
	{
		using var cnn = CreateConnection();
		
		var movies = (await cnn.QueryAsync<MovieGetFolded>(sql, new { from, to })).ToList(); // TODO twice toList??? 
		
		return new MovieWrap { Count = movies.Count, Movies = movies };
	} 
	
	
	public Task<MovieByWrap> GetMoviesBy(GetBy arg, string name) // TODO
	{
		string sql;
		
		switch (arg)
		{
			case GetBy.Actor:
				sql = MovieSql.GetByActor;
				name = name.Replace('_', ' ');
				break;
			case GetBy.Genre:
				sql = MovieSql.GetByGenre;
				break;
			case GetBy.Country:
				throw new NotImplementedException();
			default:
				throw new ArgumentException("there is no way it can reach here");
		}
		
		return GetMoviesByPrivate(sql, name);
	}
	
	private async Task<MovieByWrap> GetMoviesByPrivate(string sql, string name)
	{
		using var cnn = CreateConnection();
		
		var movies = (await cnn.QueryAsync<MovieBy>(sql, new { name })).ToList();
		
		var result = new MovieByWrap { Count = movies.Count, Movies = movies };
		
		return result;
	}
	
	public Task<bool> SaveMovie(MoviePost movie) // json from the body
	{
		List<Actor> actors = movie.Actors;
		List<Genre> genres = movie.Genres;
		
		// adding link for the movie
		var link = movie.Title.Replace(' ', '_');
		movie.Link = $"https://localhost:7777/Movie/{link}/{movie.Released}";
		
		// adding Links and Info about actors
		foreach(var actor in actors)
		{
			var replaced = actor.Name.Replace(' ', '_');
			actor.Link = $"https://localhost:7777/Movie/Actor/{replaced}";
			
			if (actor.Info == "")
				actor.Info = $"https://en.wikipedia.org/wiki/{replaced}";
		}
		
		// adding Links for genres
		foreach(var genre in genres)
			genre.Link = $"https://localhost:7777/Movie/Genre/{genre.Type}";
		
		// arrays to store id's 
		var actorIds = new ActorIds[actors.Count];
		var genreIds = new GenreIds[genres.Count];
		
		return SaveMovie(movie, actors, genres, actorIds, genreIds);
	}
	
	private Task<bool> SaveMovie(MoviePost movie, List<Actor> actors, List<Genre> genres, ActorIds[] actorIds, GenreIds[] genreIds)
	{
		try
		{
			using var cnn = CreateConnection();
			cnn.Open();

			using var transaction = cnn.BeginTransaction();
			// save movie and return id
			var movieId = transaction.QueryFirstOrDefault<int>(MovieSql.SaveMovie, movie); 
				
			// if returned 0, then there was a duplicate (Title and Released are unique)
			if (movieId == 0)
				return Task.FromResult(false);
				
			// saving each actor returning id's
			var j = 0;
			foreach (var actor in actors)
			{
				var id = transaction.QueryFirst<int>(MovieSql.SaveActors, new { actor.Name, actor.Info, actor.Link });
				actorIds[j++] = new ActorIds { Id = id };
			}
				
			// doing the same for genres
			j = 0;
			foreach(var genre in genres)
			{
				var id = transaction.QueryFirst<int>(MovieSql.SaveGenres, new { genre.Type, genre.Link });
				genreIds[j++] = new GenreIds { Id = id };
			}
				
			// saving each actors' id 
			foreach(var actor in actorIds)
				transaction.Execute(MovieSql.LinkActorMovie, new { actor.Id, movieId });
				
			// doing the same for genres
			foreach(var genre in genreIds)
				transaction.Execute(MovieSql.LinkGenreMovie, new { genre.Id, movieId });
				
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