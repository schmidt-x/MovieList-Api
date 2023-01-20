namespace MovieApi.SqlQueries;

public static class ActorSql
{
	public const string GetAll = @"
		SELECT Id, Name, Info FROM Actor"; 
	
	public const string Save = @"
		DECLARE @actorId INT = NULL
		MERGE INTO Actor AS t
		USING (SELECT @Name) AS s(Name)
		ON t.Name = s.Name
		WHEN MATCHED THEN
			UPDATE SET @actorId = t.Id
		WHEN NOT MATCHED THEN
			INSERT (Name, Info) VALUES (s.Name, @Info);
		SET @actorId = ISNULL(@actorId, SCOPE_IDENTITY())
		INSERT INTO ActorMovie (Actor_id, Movie_id) VALUES (@actorId, @movieId)";
	
	public const string Delete = @"
		DELETE FROM ActorMovie WHERE Actor_id = @actorId
		DELETE FROM Actor WHERE Id = @actorId";
}