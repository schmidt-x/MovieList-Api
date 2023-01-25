using MovieApi.DTOs;

namespace MovieApi.Services;

public interface IMemberService
{
	Task<Wrap<T>> GetAllAsync<T>(string sql);
	Task<int> DeleteAsync(int[] ids, string sql);
	// Task<string> SaveAsync<T>(T member, string sql);
}