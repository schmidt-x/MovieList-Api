using MovieApi.Models;

namespace MovieApi.Services;

public interface IMovieService
{
	Task<List<MovieGet>> GetMovies(string? title = null, int? released = null);
	Task<List<GetMoviesBy>> GetMoviesBy(GetByArg arg, string name);
	Task<bool> SaveMovie(MoviePost movie);
}