using System.Data;
using System.Data.SqlClient;
using Dapper;
using Microsoft.Extensions.Configuration;
using MovieApi.DTOs;

namespace MovieApi.Repositories;

public class MemberRepository : IMemberRepository
{
	private readonly string? _connectionString;
	private IDbConnection CreateConnection() => new SqlConnection(_connectionString);
	public MemberRepository(IConfiguration config)
	{
		_connectionString = config.GetConnectionString("Default");
	}
	
	public async Task<T?> QueryAsync<T>(int id, string sql)
	{
		using var cnn = CreateConnection();
		
		var actor = await cnn.QueryFirstOrDefaultAsync<T>(sql, new { memberId = id });
		
		return actor;
	}
	public async Task<Wrap<T>> QueryAllAsync<T>(string sql)
	{
		using var cnn = CreateConnection();
		
		var actors = (await cnn.QueryAsync<T>(sql)).ToList();
		
		return new Wrap<T>{ Count = actors.Count, List = actors };
	}
	public async Task<int> QuerySaveAsync<T>(T member, string sql)
	{
		using var cnn = CreateConnection();
		using var multi = await cnn.QueryMultipleAsync(sql, member);
		
		var isMatched = multi.ReadFirst<bool>();
		var id = multi.ReadFirst<int>();
		
		if (isMatched)
			id *= -1;
		
		return id;
	}
}