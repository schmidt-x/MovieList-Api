using MovieApi.Models;

namespace MovieApi.Services;

public interface IActorService
{
	Task<ActorWrap> GetActorsAsync();
}