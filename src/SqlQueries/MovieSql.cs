namespace MovieApi.SqlQueries;

public static class MovieSql
{
	public static string GetMovies =>
		@"select Movie.Id, Movie.Title, Movie.Duration, Movie.Released, Movie.Country, 
						 Actor.Id, Actor.Name, Actor.Info, Actor.Link, 
						 Genre.Type, Genre.Link
			from Movie
			left outer join ActorMovie am on Movie.Id = am.Movie_id
			left outer join Actor on Actor.Id = am.Actor_id
			left outer join GenreMovie gm on Movie.Id = gm.Movie_id
			left outer join Genre on Genre.Id = gm.Genre_id";
	
	public static string GetMoviesFolded =>
		@"SELECT Title, Released, Link FROM Movie";
	
	public static string SaveMovie =>
		@"INSERT INTO Movie (Title, Country, Duration, Released, Link)
				OUTPUT inserted.Id 
				VALUES (@Title, @Country, @Duration, @Released, @Link)";
	
	public static string SaveActors => 
		@"MERGE INTO Actor as t
			USING (SELECT @Name) AS s(name)
			ON t.Name = s.name
			WHEN MATCHED THEN
				UPDATE SET t.Name = s.name
			WHEN NOT MATCHED THEN
				INSERT (Name, Info, Link) VALUES (s.name, @Info, @Link)
			OUTPUT inserted.Id;";
	
	public static string SaveGenres => 
		@"MERGE INTO Genre AS t
			USING (SELECT @Type) AS s(type)
			ON t.Type = s.type
			WHEN MATCHED THEN
				UPDATE SET t.Type = s.type
			WHEN NOT MATCHED THEN
				INSERT (Type, Link) VALUES (s.type, @Link)
			OUTPUT inserted.Id;";
	
	public static string LinkActorMovie => 
		@"INSERT INTO ActorMovie (Actor_id, Movie_id) 
				VALUES (@Id, @movieId)";
	
	public static string LinkGenreMovie => 
		@"INSERT INTO GenreMovie (Genre_id, Movie_id) 
				VALUES (@Id, @movieId)";
	
	public static string GetByActor =>
		@"select Movie.Title, Movie.Link from Movie
				 inner join ActorMovie am on Movie.Id = am.Movie_id
				 inner join Actor on Actor.Id = am.Actor_id
				 where Actor.Name = @name";
	
	public static string GetByGenre =>
		@"select Movie.Title, Movie.Link  from Movie
				inner join GenreMovie gm on Movie.Id = gm.Movie_id
				inner join Genre on Genre.Id = gm.Genre_id
				where Genre.Type = @name";
				
	
}