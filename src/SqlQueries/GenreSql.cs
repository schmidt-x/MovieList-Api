namespace MovieApi.SqlQueries;

public static class GenreSql
{
	public const string GetAll = @"
		SELECT Id, Type FROM Genre";
	
	public const string SaveAndAttach = @"
		DECLARE @genreId INT = NULL
		MERGE INTO Genre AS t
			USING (SELECT @Type) AS s(Type)
		ON t.Type = s.Type
		WHEN MATCHED THEN 
			UPDATE SET @genreId = t.Id
		WHEN NOT MATCHED THEN
			INSERT (Type) VALUES (s.Type);
		SET @genreId = ISNULL(@genreId, SCOPE_IDENTITY())
		INSERT INTO GenreMovie (Genre_id, Movie_id) VALUES (@genreId, @movieId)";
	
	public const string Attach = @"
		IF EXISTS (SELECT 1 FROM Genre WHERE Id = @genreId)
			INSERT INTO GenreMovie (Genre_id, Movie_id) VALUES (@genreId, @movieId)";
		
	public const string Delete = @"
		DELETE FROM GenreMovie WHERE Genre_id = @actorId
		DELETE FROM Genre WHERE Id = @actorId";
	
	
}