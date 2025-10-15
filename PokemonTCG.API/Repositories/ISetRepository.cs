using PokemonTCG.API.DTOs;
using PokemonTCG.API.Models;

namespace PokemonTCG.API.Repositories
{
    public interface ISetRepository
    {
        Task<List<Set>> GetSetByNameAsync(string name);
        Task<List<Set>> GetSetByIdAsync(string id);
        Task<List<Set>> GetSetBySerieAsync(string serie);
        Task<List<SetDetailDTO>> GetAllSetsAsync();
        Task<Set> SaveSetAsync(Models.Set set, CancellationToken cancellationToken = default);
    }
}
