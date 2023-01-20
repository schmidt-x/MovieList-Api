using System.Data;
using System.Data.SqlClient;
using Dapper;
using Microsoft.Extensions.Configuration;
using MovieApi.DTOs;
using Serilog;

namespace MovieApi.Services;

public class MemberService : IMemberService
{
	private readonly string? _connectionString;
	private IDbConnection CreateConnection() => new SqlConnection(_connectionString);
	public MemberService(IConfiguration config) =>
		_connectionString = config.GetConnectionString("Default");
		
	
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
			Log.Error(e, "Something went wrong");
			throw;
		}
	}
	
	public async Task<bool> DeleteAsync(int id, string sql)
	{
		try
		{
			using var cnn = CreateConnection();
			var isDeleted = (await cnn.ExecuteAsync(sql, new { actorId = id })) != 0;
			return isDeleted;
		}
		catch(Exception ex)
		{
			Log.Error(ex, "Something went wrong");
			throw;
		}
	}
	
	public async Task<string> SaveAsync<T>(T member, string sql)
	{
		try
		{
			using var cnn = CreateConnection();
			
			
		}
		catch(Exception ex)
		{
			
			throw;
		}
		throw new NotImplementedException();
	}
}