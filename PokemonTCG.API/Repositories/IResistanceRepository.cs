using PokemonTCG.API.Models;

namespace PokemonTCG.API.Repositories
{
    public interface IResistanceRepository
    {
        Task<List<Resistance>> GetResistancesByCardIdAsync(string cardId);
    }
}
