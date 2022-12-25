using MovieApi.Models;

namespace MovieApi.Services;

public interface IGenreService
{
	Task<GenreGet> GetGenresAsync();
}