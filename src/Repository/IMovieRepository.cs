using MovieApi.DTOs;

namespace MovieApi.Repository;

public interface IMovieRepository
{
	Task<MovieGet?> QueryGetAsync(int id);
	Task<Wrap<MoviesGet>> QueryGetAllAsync(string sql, int? from, int? to, int? byId = null);
	Task<int> QuerySaveAsync(MoviePost movie, int[] actorId, int[] genreId);
	Task QueryDeleteAsync(int[] movieId);
	Task<MovieGet> QueryUpdateAsync(MoviePost movie, int[] actorIdDel, int[] genreIdDel, int[] actorIdAdd, int[] genreIdAdd);
}