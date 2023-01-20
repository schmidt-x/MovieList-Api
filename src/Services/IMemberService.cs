using MovieApi.DTOs;

namespace MovieApi.Services;

public interface IMemberService
{
	Task<Wrap<T>> GetAllAsync<T>(string sql);
	Task<bool> DeleteAsync(int id, string sql);
	Task<string> SaveAsync<T>(T member, string sql);
}