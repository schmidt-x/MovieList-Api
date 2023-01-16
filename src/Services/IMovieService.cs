using MovieApi.Models;

namespace MovieApi.Services;

public interface IMovieService
{
	Task<MovieGet?> GetByIdAsync(int id);
	Task<MovieWrap> GetAllAsync(int? from, int? to, string? orderBy, bool desc);
	Task<MovieWrap> GetAllByAsync(int id, GetBy arg, int? from, int? to, string? orderBy, bool desc);
	Task<string> SaveMovie(MoviePost movie);
}