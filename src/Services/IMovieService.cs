using MovieApi.Models;

namespace MovieApi.Services;

public interface IMovieService
{
	Task<MovieGetUnfolded?> GetSingleAsync(string title, int? released);
	Task<MovieWrap> GetAllAsync(int? from, int? to, bool unfolded, string? orderBy, bool desc);
	Task<MovieWrap> GetAllByAsync(GetBy arg, string byArg, string? orderBy, bool desc);
	Task<bool> SaveMovie(MoviePost movie);
}