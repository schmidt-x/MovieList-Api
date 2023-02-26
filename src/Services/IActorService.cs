using MovieApi.DTOs;

namespace MovieApi.Services;

public interface IActorService
{
	Task<ActorGet?> GetAsync(int id);
	Task<Wrap<ActorGet>> GetAllAsync();
	Task<int> SaveAsync(ActorPost actor);
}