using MovieApi.DTOs;
using MovieApi.Repositories;
using MovieApi.SqlQueries;

namespace MovieApi.Services;

public class ActorService : IActorService
{
	private readonly IMemberRepository _mRepo;
	public ActorService(IMemberRepository mRepo)
	{
		_mRepo = mRepo;
	}
	
	public Task<ActorGet?> GetAsync(int id)
	{
		// ..
		
		return _mRepo.QueryAsync<ActorGet>(id, ActorSql.Get);
	}
	public Task<Wrap<ActorGet>> GetAllAsync()
	{
		// ...
		
		return _mRepo.QueryAllAsync<ActorGet>(ActorSql.GetAll);
	}
	public Task<int> SaveAsync(ActorPost actor)
	{
		if (string.IsNullOrWhiteSpace(actor.Name))
			throw new ArgumentNullException(nameof(actor.Name), "Field 'Name' cannot be empty!");
		
		return _mRepo.QuerySaveAsync(actor, ActorSql.Save);
	}
	
}