namespace MovieApi.SqlQueries;

public static class MovieSql
{
	public const string GetAll =
		@"SELECT Id, Title, Duration, Released, Link From Movie";
		
	public const string GetById =
		@"SELECT Movie.Id, Movie.Title, Movie.Duration, Movie.Released, Movie.Country, Movie.About,
						 Actor.Id, Actor.Name, Actor.Info, Actor.Link, 
						 Genre.Id, Genre.Type, Genre.Link
			FROM Movie
			LEFT OUTER JOIN ActorMovie am ON Movie.Id = am.Movie_id
			LEFT OUTER JOIN Actor ON Actor.Id = am.Actor_id
			LEFT OUTER JOIN GenreMovie gm ON Movie.Id = gm.Movie_id
			LEFT OUTER JOIN Genre ON Genre.Id = gm.Genre_id
			WHERE Movie.Id = @movieId";
	
	public const string SaveMovie = @"
		DECLARE @match BIT = 0, @movieId INT
		MERGE INTO Movie AS t
			USING (select @Title, @Released) as s(Title, Released)
		ON t.Title = s.Title AND t.Released = s.Released
		WHEN MATCHED THEN
			UPDATE SET @match = 1, @movieId = t.Id
		WHEN NOT MATCHED THEN
			INSERT (Title, Duration, Released, Country)
			VALUES (s.Title, @Duration, s.Released, @Country);
		IF @match = 0
			BEGIN
				SET @movieId = SCOPE_IDENTITY()
				UPDATE Movie SET Link = 'https://localhost:7777/Movie/' + CONVERT(nvarchar(100), @movieId) WHERE Id = @movieId
			END
		SELECT @match
		SELECT @movieId";
	
	public const string SaveActors = @"
		DECLARE @match BIT = 0, @actorId INT
		MERGE INTO Actor AS t
			USING (SELECT @Name) AS s(Name)
			ON t.Name = s.Name
		WHEN MATCHED THEN
			UPDATE SET @match = 1, @actorId = t.Id
		WHEN NOT MATCHED THEN
			INSERT (Name, Info) VALUES (s.Name, @Info);
		IF @match = 0
			BEGIN
				SET @actorId = SCOPE_IDENTITY()
				UPDATE Actor SET Link = 'https://localhost:7777/Movie/byActor/' + CONVERT(NVARCHAR(100), @actorId)
					WHERE Id = @actorId
			END
		INSERT INTO ActorMovie (Actor_id, Movie_id) VALUES (@actorId, @movieId)";
	
	public const string SaveGenres = @"
		DECLARE @match BIT = 0, @genreId INT
		MERGE INTO Genre AS t
			USING (SELECT @Type) AS s(Type)
			ON t.Type = s.Type
		WHEN MATCHED THEN 
			UPDATE SET @match = 1, @genreId = t.Id
		WHEN NOT MATCHED THEN
			INSERT (Type) VALUES (s.Type);
		IF @match = 0
			BEGIN
				SET @genreId = SCOPE_IDENTITY()
				UPDATE Genre SET Link = 'https://localhost:7777/Movie/byGenre/' + CONVERT(NVARCHAR(100), @genreId)
					WHERE Id = @genreId
			END
		INSERT INTO GenreMovie (Genre_id, Movie_id) VALUES (@genreId, @movieId)";
	
	public const string ByActor = @"
		SELECT Movie.Id, Title, Duration, Released, Movie.Link FROM Movie
		INNER JOIN ActorMovie am ON Movie.Id = am.Movie_id
		INNER JOIN Actor ON Actor.Id = am.Actor_id
		WHERE Actor.Id = @byId";
			
	public const string ByGenre = @"
		SELECT Movie.Id, Title, Duration, Released, Movie.Link FROM Movie
		INNER JOIN GenreMovie am ON Movie.Id = am.Movie_id
		INNER JOIN Genre ON Genre.Id = am.Genre_id
		WHERE Genre.Id = @byId";
	
	
	
}