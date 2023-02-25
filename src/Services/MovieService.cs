using System.Data;
using System.Data.SqlClient;
using Dapper;
using Dapper.Transaction;
using Microsoft.Extensions.Configuration;
using MovieApi.DTOs;
using MovieApi.SqlQueries;
using Microsoft.Extensions.Logging;
using MovieApi.Enums;
using MovieApi.Repository;

namespace MovieApi.Services;

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
	} // DONE
	public Task<Wrap<MoviesGet>> GetAllAsync(int? from, int? to, string? orderBy, bool desc)
	{
		var sql = MovieSql.GetAll;
		sql += AddCondAndOrdering(from, to, false, orderBy, desc);
		
		return _mRepo.QueryGetAllAsync(sql, from, to);
	} // DONE
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
	} // DONE
	public Task<int> SaveAsync(MoviePost movie, int[] actorId, int[] genreId)
	{
		ValidateMovie(movie);
		
		return _mRepo.QuerySaveAsync(movie, actorId, genreId);
	}  // DONE
	public Task DeleteAsync(int[] movieId)
	{
		// ..
		
		return _mRepo.QueryDeleteAsync(movieId);
	} // DONE
	
	public Task<MovieGet> UpdateAsync(MoviePost movie, int[] actorIdDel, int[] genreIdDel, int[] actorIdAdd, int[] genreIdAdd)
	{
		ValidateMovie(movie);
		
		return _mRepo.QueryUpdateAsync(movie, actorIdDel, genreIdDel, actorIdAdd, genreIdAdd);
	} // TODO
	
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
	private void ValidateMovie(MoviePost movie)
	{
		if (string.IsNullOrWhiteSpace(movie.Title))
			throw new ArgumentNullException(nameof(movie.Title), "Value 'Title' cannot be empty!");
		
		if (movie.Release < 1896)
			throw new ArgumentException("Value 'Release' cannot be less than 1896!", nameof(movie.Release));
	}
}