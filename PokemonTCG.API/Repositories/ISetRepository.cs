using PokemonTCG.API.DTOs;
using PokemonTCG.API.Models;

namespace PokemonTCG.API.Repositories
{
    public interface ISetRepository
    {
        Task<List<SetDetailDTO>> GetSetByNameAsync(string name);
        Task<List<SetDetailDTO>> GetSetByIdAsync(string id);
        Task<List<SetDetailDTO>> GetSetBySerieAsync(string serie);
        Task<List<SetDetailDTO>> GetAllSetsAsync();
        Task<Set> SaveSetAsync(Models.Set set, CancellationToken cancellationToken = default);
    }
}
