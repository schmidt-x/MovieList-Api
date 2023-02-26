using MovieApi.DTOs;

namespace MovieApi.Repositories;

public interface IMovieRepository
{
	Task<MovieGet?> QueryGetAsync(int id);
	Task<Wrap<MoviesGet>> QueryGetAllAsync(string sql, int? from, int? to, int? byId = null);
	Task<int> QuerySaveAsync(MoviePost movie, int[] actorId, int[] genreId);
	Task<bool> QueryDeleteAsync(int[] movieId);
	Task<bool> QueryUpdateAsync(int id, MoviePut movie, int[] actorIdDel, int[] genreIdDel, int[] actorIdAdd, int[] genreIdAdd);
}