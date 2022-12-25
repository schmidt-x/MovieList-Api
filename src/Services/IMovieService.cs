using MovieApi.Models;

namespace MovieApi.Services;

public interface IMovieService
{
	Task<MainMovie> GetMovies(string? title = null, int? released = null);
	Task<MoviesBy> GetMoviesBy(GetByArg arg, string name);
	Task<bool> SaveMovie(MoviePost movie);
}