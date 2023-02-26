namespace MovieApi.SqlQueries;

public static class MovieSql
{
	public const string GetAll = @"
		SELECT Id, Title, Release, Country, Duration From Movie";
	
	public const string GetAllByActor = @"
		SELECT Movie.Id, Title, Release, Country, Duration FROM Movie
		INNER JOIN ActorMovie am ON Movie.Id = am.Movie_id
		INNER JOIN Actor ON Actor.Id = am.Actor_id
		WHERE Actor.Id = @byId";
			
	public const string GetAllByGenre = @"
		SELECT Movie.Id, Title, Release, Country, Duration FROM Movie
		INNER JOIN GenreMovie am ON Movie.Id = am.Movie_id
		INNER JOIN Genre ON Genre.Id = am.Genre_id
		WHERE Genre.Id = @byId";
		
	public const string Get = @"
		SELECT Movie.Id, Movie.Title, Movie.Release, Movie.Country, Movie.Duration, Movie.About,
					 Actor.Id, Actor.Name, Actor.Info, 
					 Genre.Id, Genre.Type
		FROM Movie
		LEFT OUTER JOIN ActorMovie am ON Movie.Id = am.Movie_id
		LEFT OUTER JOIN Actor ON Actor.Id = am.Actor_id
		LEFT OUTER JOIN GenreMovie gm ON Movie.Id = gm.Movie_id
		LEFT OUTER JOIN Genre ON Genre.Id = gm.Genre_id
		WHERE Movie.Id = @movieId";
		
	public const string Save = @"
		DECLARE @isMatched BIT = 0, @movieId INT = NULL
		MERGE INTO Movie AS t
		USING (select @Title, @Release) as s(Title, Release)
		ON t.Title = s.Title AND t.Release = s.Release
		WHEN MATCHED THEN
			UPDATE SET @isMatched = 1, @movieId = t.Id
		WHEN NOT MATCHED THEN
			INSERT (Title, Release, Country, Duration, About)
			VALUES (s.Title, s.Release, @Country, @Duration, @About);
		SELECT @isMatched
		SELECT ISNULL(@movieId, SCOPE_IDENTITY())";
	
	public const string Delete = @"
		DELETE FROM ActorMovie WHERE Movie_id = @movieId
		DELETE FROM GenreMovie WHERE Movie_id = @movieId
		DELETE FROM Movie WHERE Id = @movieId";
	
	public const string Update = @" 
		IF EXISTS (SELECT 1 FROM Movie WHERE Id = @movieId)
		BEGIN
			UPDATE Movie
			SET Title = @Title, Release = @Release, Country = @Country, Duration = @Duration, About = @About
			WHERE Id = @movieId
			SELECT 1
		END
		ELSE
			SELECT 0";
}