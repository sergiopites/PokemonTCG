using PokemonTCG.API.Models;

namespace PokemonTCG.API.Services
{
    public interface IResistanceService
    {
        Task<List<Resistance>> GetResistanceByCardId(string cardId);
    }
}
