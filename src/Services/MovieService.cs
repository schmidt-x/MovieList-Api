using System.Data.SqlClient;
using Dapper;
using Dapper.Transaction;
using Microsoft.Extensions.Configuration;
using MovieApi.Models;

namespace MovieApi.Services;

public class MovieService : IMovieService
{
	private readonly string _connectionString;
	public MovieService(IConfiguration config) => 
		_connectionString = config.GetConnectionString("Default"); 
	
	public Task<MainMovie> GetMovies(string? title = null, int? released = null)
	{
		// if title and released are null, then is't called from Get/Movies and need to return all movies
		var sql = 
			@"select Movie.Id, Movie.Title, Movie.Duration, Movie.Released, Movie.Country, 
					Actor.Id, Actor.Name, Actor.Info, Actor.Link, 
					Genre.Type, Genre.Link
				from Movie
				left outer join ActorMovie am on Movie.Id = am.Movie_id
				left outer join Actor on Actor.Id = am.Actor_id
				left outer join GenreMovie gm on Movie.Id = gm.Movie_id
				left outer join Genre on Genre.Id = gm.Genre_id";
		
		if (title != null)
		{
			title = title.Replace('_', ' ');
			sql += " where Movie.Title = @title";
			if (released != null)
				sql += " and Movie.Released = @released";
		}
		
		return GetMoviesPrivate(sql, title, released);
	}
	
	private async Task<MainMovie> GetMoviesPrivate(string sql, string? title, int? released)
	{
		using var db = new SqlConnection(_connectionString);
		
		var movies = await db.QueryAsync<MovieGet, Actor, Genre, MovieGet>(sql, (movie, actor, genre) =>
		{
			if (actor != null)
				movie.Actors = new() { actor };
			if (genre != null)
				movie.Genres = new() { genre };
			
			return movie;
		}, new { title, released } , splitOn: "Id, Type");
		
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
					.DistinctBy(a => a.Type) // someday add by Id
					.ToList();
			}
			catch(Exception)
			{
				movie.Genres = null;
			}
			
			return movie;
			
		}).ToList();
		
		return new MainMovie { Count = result.Count, Movies = result };
	}
	
	public Task<MoviesBy> GetMoviesBy(GetByArg arg, string name)
	{
		var sqlByActor = 
		@"select Movie.Title, Movie.Link from Movie
				 inner join ActorMovie am on Movie.Id = am.Movie_id
				 inner join Actor on Actor.Id = am.Actor_id
				 where Actor.Name = @name";
		
		var sqlByGenre = 
			@"select Movie.Title, Movie.Link  from Movie
				inner join GenreMovie gm on Movie.Id = gm.Movie_id
				inner join Genre on Genre.Id = gm.Genre_id
				where Genre.Type = @name";
		
		if (arg == GetByArg.Genre) 
			return GetMoviesByPrivate(sqlByGenre, name);
		
		name = name.Replace('_', ' ');
		return GetMoviesByPrivate(sqlByActor, name);
		
	}
	
	private async Task<MoviesBy> GetMoviesByPrivate(string sql, string name)
	{
		using var db = new SqlConnection(_connectionString);
		
		var movies = (await db.QueryAsync<MovieBy>(sql, new { name })).ToList();
		
		var result = new MoviesBy { Count = movies.Count, Movies = movies };
		
		return result;
	}
	
	public Task<bool> SaveMovie(MoviePost movie) // json from the body
	{
		#region SqlCommands
		
		const string saveMovieSql = 
			@"INSERT INTO Movie (Title, Country, Duration, Released, Link)
				OUTPUT inserted.Id 
				VALUES (@Title, @Country, @Duration, @Released, @Link)";
		
		const string saveActorsSql = 
			@"MERGE INTO Actor as t
				USING (SELECT @Name) AS s(name)
				ON t.Name = s.name
				WHEN MATCHED THEN
					UPDATE SET t.Name = s.name
				WHEN NOT MATCHED THEN
					INSERT (Name, Info, Link) VALUES (s.name, @Info, @Link)
				OUTPUT inserted.Id;";
		
		const string saveGenresSql = 
			@"MERGE INTO Genre AS t
				USING (SELECT @Type) AS s(type)
				ON t.Type = s.type
				WHEN MATCHED THEN
					UPDATE SET t.Type = s.type
				WHEN NOT MATCHED THEN
					INSERT (Type, Link) VALUES (s.type, @Link)
				OUTPUT inserted.Id;";
		
		const string attachActorMovieSql = 
			@"INSERT INTO ActorMovie (Actor_id, Movie_id) 
				VALUES (@Id, @movieId)";
			
		const string attachGenreMovieSql = 
			@"INSERT INTO GenreMovie (Genre_id, Movie_id) 
				VALUES (@Id, @movieId)";
		
		#endregion
		
		List<Actor> actors = movie.Actors;
		List<Genre> genres = movie.Genres;
		
		// adding link for the movie
		var link = movie.Title.Replace(' ', '_');
		movie.Link = $"https://localhost:7777/Movies/{link}/{movie.Released}";
		
		// adding Links and Info about actors
		foreach(var actor in actors)
		{
			var replaced = actor.Name.Replace(' ', '_');
			actor.Link = $"https://localhost:7777/Movies/Actor/{replaced}";
			if (actor.Info == "")
				actor.Info = $"https://en.wikipedia.org/wiki/{replaced}";
		}
		
		// adding Links for genres
		foreach(var genre in genres)
		{
			genre.Link = $"https://localhost:7777/Movies/Genre/{genre.Type}";
		}
		
		// arrays to keep id's 
		var actorIds = new ActorIds[actors.Count];
		var genreIds = new GenreIds[genres.Count];
		
		using var connection = new SqlConnection(_connectionString);
		connection.Open();
		
		using (var transaction = connection.BeginTransaction())
		{
			// save movie and return id
			var movieId = transaction.QueryFirstOrDefault<int>(saveMovieSql, movie); 
			
			// if returned 0, then there was a duplicate (Title and Released are unique)
			if (movieId == 0)
				return Task.FromResult(false);
			
			// saving each actor and getting id's
			var j = 0;
			foreach (var actor in actors)
			{
				var id = transaction.QueryFirst<int>(saveActorsSql, new { actor.Name, actor.Info, actor.Link });
				actorIds[j++] = new ActorIds { Id = id };
			}
			
			// doing the same for genres
			j = 0;
			foreach(var genre in genres)
			{
				var id = transaction.QueryFirst<int>(saveGenresSql, new { genre.Type, genre.Link });
				genreIds[j++] = new GenreIds { Id = id };
			}
			
			// saving each actors' id 
			foreach(var actor in actorIds)
				transaction.Execute(attachActorMovieSql, new { actor.Id, movieId });
			
			// doing the same for genres
			foreach(var genre in genreIds)
				transaction.Execute(attachGenreMovieSql, new { genre.Id, movieId});
			
			transaction.Commit();
		}
		
		return Task.FromResult(true);
	}
	
}

public enum GetByArg
{
	Actor = 1,
	Genre
}