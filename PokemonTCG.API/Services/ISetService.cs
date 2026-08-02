using PokemonTCG.API.Models;
using PokemonTCG.API.Responses;

namespace PokemonTCG.API.Services
{
    public interface ISetService
    {
        Task<List<SetDetailResponse>> GetAllSetsAsync();
        Task<List<SetDetailResponse>> GetSetByIdAsync(string id);
        Task<List<SetDetailResponse>> GetSetByNameAsync(string name);
        Task<List<SetDetailResponse>> GetSetBySerieAsync(string series);
        Task SaveSetAsync(CancellationToken cancellationToken);
        Task SaveSetAsync(Func<string, Task> log, CancellationToken cancellationToken);
        Task<List<Models.Set>> GetAllPokemonSetsAsync();
    }
}
