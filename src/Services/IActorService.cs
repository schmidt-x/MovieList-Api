using MovieApi.Models;

namespace MovieApi.Services;

public interface IActorService
{
	Task<ActorGet> GetActorsAsync();
}