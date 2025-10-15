using PokemonTCG.API.Models;
using PokemonTCG.API.Responses;

namespace PokemonTCG.API.Services
{
    public interface ISetService
    {
        Task<List<SetDetailResponse>> GetAllSetsAsync();
        Task<List<Set>> GetSetByIdAsync(string id);
        Task<List<Set>> GetSetByNameAsync(string name);
        Task<List<Set>> GetSetBySerieAsync(string series);
        Task SaveSetAsync(CancellationToken cancellationToken);
        Task<List<Models.Set>> GetAllPokemonSetsAsync();
    }
}
