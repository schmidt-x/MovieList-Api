using MovieApi.Models;

namespace MovieApi.Services;

public interface IService
{
	Task<List<Movie>> GetMovies(string? title = null, int? released = null);
	Task<List<GetMoviesBy>> GetMoviesBy(GetByArg arg, string name);
	Task<bool> SaveMovie(MoviePost movie);
}