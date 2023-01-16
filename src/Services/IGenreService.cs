using MovieApi.Models;

namespace MovieApi.Services;

public interface IGenreService
{
	Task<GenreWrap> GetGenresAsync();
}