using System.Data.SqlClient;
using Dapper;
using Dapper.Transaction;
using Microsoft.Extensions.Configuration;
using MovieApi.Models;

namespace MovieApi.Services;

public class MovieService : IService
{
	private readonly string _connectionString;
	public MovieService(IConfiguration config) => 
		_connectionString = config.GetConnectionString("Default"); 
	
	public Task<List<Movie>> GetMovies(string? title = null, int? released = null)
	{
		// if title and released are null, then is't called from Get/Movies and need to return all movies
		var sql = 
			@"select Movie.Id, Movie.Title, Movie.Duration, Movie.Released, Movie.Country, Actor.Id, Actor.Name, Actor.Info, Genre.Type
				from Movie
				left outer join ActorMovie am on Movie.Id = am.Movie_id
				left outer join Actor on Actor.Id = am.Actor_id
				left outer join GenreMovie gm on Movie.Id = gm.Movie_id
				left outer join Genre on Genre.Id = gm.Genre_id";
		
		if (title != null)
		{
			title = title.Replace('_', ' ');
			sql += " where Movie.Title = @name";
			if (released != null)
				sql += " and Movie.Released = @age";
		}
		
		return GetMoviesPrivate(sql, title, released);
	}
	
	private async Task<List<Movie>> GetMoviesPrivate(string sql, string? title, int? released)
	{
		using var db = new SqlConnection(_connectionString);
		
		var movies = await db.QueryAsync<Movie, Actor, string, Movie>(sql, (movie, actor, genre) =>
		{
			if (actor != null)
				movie.Actors = new() { actor };
			if (genre != null)
				movie.Genres = new() { genre };
			
			return movie;
		}, new { name = title, age = released } , splitOn: "Id, Type");
		
		var result = movies.GroupBy(m => m.Id).Select(g =>
		{
			var movie = g.First();
			
			try
			{
				movie.Actors = g
					.Select(m => m.Actors!
					.Single())
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
					.Distinct()
					.ToList();
			}
			catch(Exception)
			{
				movie.Genres = null;
			}
			
			return movie;
			
		}).ToList();
		
		return result;
	}
	
	public Task<List<GetMoviesBy>> GetMoviesBy(GetByArg arg, string name)
	{
		var sqlByActor = 
		@"select Movie.Title from Movie
				 inner join ActorMovie am on Movie.Id = am.Movie_id
				 inner join Actor on Actor.Id = am.Actor_id
				 where Actor.Name = @name";
		
		var sqlByGenre = 
			@"select Movie.Title from Movie
				inner join GenreMovie gm on Movie.Id = gm.Movie_id
				inner join Genre on Genre.Id = gm.Genre_id
				where Genre.Type = @name";
		
		if (arg == GetByArg.Genre) 
			return GetMoviesByPrivate(sqlByGenre, name);
		
		name = name.Replace('_', ' ');
		return GetMoviesByPrivate(sqlByActor, name);
		
	}
	
	private async Task<List<GetMoviesBy>> GetMoviesByPrivate(string sql, string name)
	{
		using var db = new SqlConnection(_connectionString);
		
		var movies = await db.QueryAsync<string>(sql, new { name });
		
		return (from movie in movies
						let link = $"https://localhost:7777/Movies/{movie.Replace(' ', '_')}"
						select new GetMoviesBy { Title = movie, Link = link }).ToList();
	}
	
	public Task<bool> SaveMovie(MoviePost movie) // json from the body
	{
		#region SqlCommands
		
		const string saveMovieSql = 
			@"INSERT INTO Movie (Title, Country, Duration, Released)
				OUTPUT inserted.Id 
				VALUES (@Title, @Country, @Duration, @Released)";
		
		const string saveActorsSql = 
			@"MERGE INTO Actor as t
				USING (SELECT @Name) AS s(name)
				ON t.Name = s.name
				WHEN MATCHED THEN
					UPDATE SET t.Name = s.name
				WHEN NOT MATCHED THEN
					INSERT (Name, Info) VALUES (s.name, @Info)
				OUTPUT inserted.Id;";
		
		const string saveGenresSql = 
			@"MERGE INTO Genre AS t
				USING (SELECT @Type) AS s(type)
				ON t.Type = s.type
				WHEN MATCHED THEN
					UPDATE SET t.Type = s.type
				WHEN NOT MATCHED THEN
					INSERT (Type) VALUES (s.type)
				OUTPUT inserted.Id;";
		
		const string attachActorMovieSql = 
			@"INSERT INTO ActorMovie (Actor_id, Movie_id) 
				VALUES (@Id, @movieId)";
			
		const string attachGenreMovieSql = 
			@"INSERT INTO GenreMovie (Genre_id, Movie_id) 
				VALUES (@Id, @movieId)";
		
		#endregion
		
		List<Actors> actors = movie.Actors;
		List<Genres> genres = movie.Genres;
		
		// just adding some basic info about actors if it's not provided
		foreach(var actor in actors.Where(act => act.Info == ""))
			actor.Info = $"https://en.wikipedia.org/wiki/{actor.Name.Replace(' ', '_')}";
		
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
			foreach (var id in actors.Select(act => transaction.QueryFirst<int>(saveActorsSql, new { act.Name, act.Info })))
			{
				actorIds[j++] = new ActorIds { Id = id };
			}
			// doing the same for genres
			j = 0;
			foreach(var genre in genres)
			{
				var id = transaction.QueryFirst<int>(saveGenresSql, new { genre.Type });
				genreIds[j++] = new GenreIds() { Id = id };
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