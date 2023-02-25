using MovieApi.DTOs;
using MovieApi.Enums;

namespace MovieApi.Services;

public interface IMovieService
{
	Task<MovieGet?> GetAsync(int id);
	Task<Wrap<MoviesGet>> GetAllAsync(int? from, int? to, string? orderBy, bool desc);
	Task<Wrap<MoviesGet>> GetAllByAsync(int id, GetBy arg, int? from, int? to, string? orderBy, bool desc);
	Task<int> SaveAsync(MoviePost movie, int[] actorId, int[] genreId);
	Task DeleteAsync(int[] movieId);
	Task<MovieGet> UpdateAsync(MoviePost movie, int[] actorIdDel, int[] genreIdDel, int[] actorIdAdd, int[] genreIdAdd);
}