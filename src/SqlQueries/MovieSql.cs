namespace MovieApi.SqlQueries;

public static class MovieSql
{
	public const string GetAll = @"
		SELECT Id, Title, Duration, Released, About From Movie";
	
	public const string GetAllByActor = @"
		SELECT Movie.Id, Title, Duration, Released, About FROM Movie
		INNER JOIN ActorMovie am ON Movie.Id = am.Movie_id
		INNER JOIN Actor ON Actor.Id = am.Actor_id
		WHERE Actor.Id = @byId";
			
	public const string GetAllByGenre = @"
		SELECT Movie.Id, Title, Duration, Released, About FROM Movie
		INNER JOIN GenreMovie am ON Movie.Id = am.Movie_id
		INNER JOIN Genre ON Genre.Id = am.Genre_id
		WHERE Genre.Id = @byId";
		
	public const string Get = @"
		SELECT Movie.Id, Movie.Title, Movie.Duration, Movie.Released, Movie.Country, Movie.About,
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
		USING (select @Title, @Released) as s(Title, Released)
		ON t.Title = s.Title AND t.Released = s.Released
		WHEN MATCHED THEN
			UPDATE SET @isMatched = 1, @movieId = t.Id
		WHEN NOT MATCHED THEN
			INSERT (Title, Duration, Released, Country, About)
			VALUES (s.Title, @Duration, s.Released, @Country, @About);
		SELECT @isMatched
		SELECT ISNULL(@movieId, SCOPE_IDENTITY())";
	
	public const string Delete = @"
		DELETE FROM ActorMovie WHERE Movie_id = @movieId
		DELETE FROM GenreMovie WHERE Movie_id = @movieId
		DELETE FROM Movie WHERE Id = @movieId";
	
}