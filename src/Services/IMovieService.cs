using MovieApi.Models;

namespace MovieApi.Services;

public interface IMovieService
{
	Task<MovieWrap> GetSingleAsync(string title, int? released = null);
	Task<MovieWrap> GetAllAsync(int? from, int? to, bool folded);
	// Task<MovieWrap> GetMoviesAsync();
	Task<MovieByWrap> GetMoviesBy(GetBy arg, string name);
	Task<bool> SaveMovie(MoviePost movie);
}