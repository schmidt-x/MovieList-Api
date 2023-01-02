namespace MovieApi.SqlQueries;

public static class MovieSql
{
	public static string GetMovies =>
		@"SELECT Title, Released, Movie.Link FROM Movie";
		
	public static string GetMoviesUnfolded =>
		@"select Movie.Id, Movie.Title, Movie.Duration, Movie.Released, Movie.Country, 
						 Actor.Id, Actor.Name, Actor.Info, Actor.Link, 
						 Genre.Type, Genre.Link
			from Movie
			LEFT OUTER JOIN ActorMovie am on Movie.Id = am.Movie_id
			LEFT OUTER JOIN Actor on Actor.Id = am.Actor_id
			LEFT OUTER JOIN GenreMovie gm on Movie.Id = gm.Movie_id
			LEFT OUTER JOIN Genre on Genre.Id = gm.Genre_id";
	
	public static string SaveMovie =>
		@"declare @movieId INT
			insert into Movie (Title, Country, Duration, Released)
				VALUES (@Title, @Country, @Duration, @Released)
			set @movieId = Scope_Identity()
			update Movie set Link = 'https://localhost:7777/Movie/' + convert(nvarchar(100), @movieId) where Id = @movieId
			select @movieId;";
	
	public static string SaveActors => 
		@"declare @actorId INT
			merge into Actor as t
				using (select @Name) as s(name)
			on t.Name = s.name
			when matched then
				update set t.Name = s.name
			when not matched then
				insert (Name, Info) values (s.name, @Info); 
			set @actorId = SCOPE_IDENTITY()
			update Actor set Link = 'https://localhost:7777/Actor/' + convert(nvarchar(100), @actorId) where Id = @actorId
			insert into ActorMovie (Actor_id, Movie_id) values (@actorId, @movieId)";
	
	public static string SaveGenres => 
		@"declare @genreId INT
		merge into Genre as t
			using (select @Type) as s(type)
		on t.Type = s.type
		when matched then
			update set t.Type = s.type
		when not matched then
			insert (Type) values (s.type);
		set @genreId = Scope_Identity()
		update Genre set Link = 'https://localhost:7777/Genre/' + convert(nvarchar(100), @genreId) where Id = @genreId
		insert into GenreMovie (Genre_id, Movie_id) values (@genreId, @movieId)";
	
	public static string ByActor =>
		@"SELECT Title, Released, Movie.Link FROM Movie
			INNER JOIN ActorMovie am ON Movie.Id = am.Movie_id
			INNER JOIN Actor ON Actor.Id = am.Actor_id
			WHERE Actor.Name = @byArg";
			
	public static string ByGenre =>
		@"SELECT Title, Released, Movie.Link FROM Movie
			INNER JOIN GenreMovie am ON Movie.Id = am.Movie_id
			INNER JOIN Genre ON Genre.Id = am.Genre_id
			WHERE Genre.Type = @byArg";
	
	
	
}