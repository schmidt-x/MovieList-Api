using MovieApi.DTOs;
using MovieApi.SqlQueries;
using MovieApi.Enums;

namespace MovieApi.Repositories;

public class MovieService : IMovieService
{
	private readonly IMovieRepository _mRepo;
	private readonly Dictionary<string, string> _orderBy;
	public MovieService(IMovieRepository mRepo)
	{
		_mRepo = mRepo;
		_orderBy = new(StringComparer.OrdinalIgnoreCase)
		{
			{"title", " ORDER BY Movie.Title"},
			{"release", " ORDER BY Movie.Release"},
			{"duration", " ORDER BY Movie.Duration"}
		};
	}
	
	public Task<MovieGet?> GetAsync(int id)
	{
		if (id <= 0) 
			return Task.FromResult<MovieGet?>(null);
		
		return _mRepo.QueryGetAsync(id);
	}
	public Task<Wrap<MoviesGet>> GetAllAsync(int? from, int? to, string? orderBy, bool desc)
	{
		var sql = MovieSql.GetAll;
		sql += AddCondAndOrdering(from, to, false, orderBy, desc);
		
		return _mRepo.QueryGetAllAsync(sql, from, to);
	}
	public Task<Wrap<MoviesGet>> GetAllByAsync(int id, GetBy arg, int? from, int? to, string? orderBy, bool desc)
	{
		string sql = arg switch
		{
			GetBy.Actor => MovieSql.GetAllByActor,
			GetBy.Genre => MovieSql.GetAllByGenre,
			_ => throw new ArgumentException("there is no way it can get here", nameof(arg))
		};
		
		sql += AddCondAndOrdering(from, to, true, orderBy, desc);
		
		return _mRepo.QueryGetAllAsync(sql, from, to, id);
	}
	public Task<int> SaveAsync(MoviePost movie, int[] actorId, int[] genreId)
	{
		ValidateMovie(movie.Title, movie.Release);
		
		return _mRepo.QuerySaveAsync(movie, actorId, genreId);
	}
	public Task<bool> DeleteAsync(int[] movieId)
	{
		// ..
		
		return _mRepo.QueryDeleteAsync(movieId);
	} 
	public Task<bool> UpdateAsync(int id, MoviePut movie, int[] actorIdDel, int[] genreIdDel, int[] actorIdAdd, int[] genreIdAdd)
	{
		ValidateMovie(movie.Title, movie.Release);	
		
		return _mRepo.QueryUpdateAsync(id, movie, actorIdDel, genreIdDel, actorIdAdd, genreIdAdd);
	}
	
	private string AddCondAndOrdering(int? from, int? to, bool hasCondition, string? orderBy, bool desc)
	{
		string sql = string.Empty;
		
		if (from != null || to != null)
		{
			sql = hasCondition ? " AND"
												 : " WHERE";
			if (from != null)
			{
				sql += " Movie.Release >= @from";
				if (to != null)
					sql += " AND Movie.Release <= @to";
			}
			else
				sql += " Movie.Release <= @to";
		}
		
		if (orderBy != null && _orderBy.TryGetValue(orderBy, out string? value))
		{
			sql += value;
			if (desc) sql += " DESC";
		}
		
		return sql;
	} 
	private void ValidateMovie(string? title, int release)
	{
		if (string.IsNullOrWhiteSpace(title))
			throw new ArgumentNullException(nameof(title), "Field 'Title' cannot be empty!");
		
		if (release < 1896)
			throw new ArgumentException("Field 'Release' cannot be less than 1896!", nameof(release));
	}
}