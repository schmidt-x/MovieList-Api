using MovieApi.DTOs;

namespace MovieApi.Services;

public interface IMovieService
{
	Task<MovieGet?> GetByIdAsync(int id);
	Task<Wrap<MoviesGet>> GetAllAsync(int? from, int? to, string? orderBy, bool desc);
	Task<Wrap<MoviesGet>> GetAllByAsync(int id, GetBy arg, int? from, int? to, string? orderBy, bool desc);
	Task<string> SaveAsync(MoviePost movie, int[] actorId, int[] genreId);
	Task<int> DeleteAsync(int[] ids);
}