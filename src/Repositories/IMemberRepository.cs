using MovieApi.DTOs;

namespace MovieApi.Repositories;

public interface IMemberRepository
{
	Task<T?> QueryAsync<T>(int id, string sql);
	Task<Wrap<T>> QueryAllAsync<T>(string sql);
	Task<int> QuerySaveAsync<T>(T member, string sql); 


}