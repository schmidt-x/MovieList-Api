using System.Data;
using System.Data.SqlClient;
using Dapper;
using Microsoft.Extensions.Configuration;
using MovieApi.DTOs;
using Microsoft.Extensions.Logging;
namespace MovieApi.Services;

public class MemberService : IMemberService
{
	private readonly ILogger<MemberService> _logger;
	private readonly string? _connectionString;
	private IDbConnection CreateConnection() => new SqlConnection(_connectionString);
	public MemberService(IConfiguration config, ILogger<MemberService> logger)
	{
		_logger = logger;
		_connectionString = config.GetConnectionString("Default");
	}
	
	
	public async Task<Wrap<T>> GetAllAsync<T>(string sql)
	{
		try
		{
			using var cnn = CreateConnection();
			var actors = (await cnn.QueryAsync<T>(sql)).ToList();
			return new Wrap<T> { Count = actors.Count, List = actors };
		}
		catch(Exception e)
		{
			_logger.LogError(e, "Failed to get the members");
			throw;
		}
	}
	
	public async Task<int> DeleteAsync(int[] ids, string sql)
	{
		try
		{
			using var cnn = CreateConnection();
			int deleted = 0;
			foreach(var id in ids)
			{
				if (await cnn.ExecuteAsync(sql, new { actorId = id }) > 0)
					deleted++;
			}
			return deleted;
		}
		catch(Exception ex)
		{
			_logger.LogError(ex, "Failed to delete the members");
			throw;
		}
	}
	
	// public async Task<string> SaveAsync<T>(T member, string sql)
	// {
	// 	try
	// 	{
	// 		using var cnn = CreateConnection();
	// 		
	// 		
	// 	}
	// 	catch(Exception ex)
	// 	{
	// 		
	// 		throw;
	// 	}
	// 	throw new NotImplementedException();
	// }
}