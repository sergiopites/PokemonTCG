using PokemonTCG.API.Models;

namespace PokemonTCG.API.Repositories
{
    public interface IAbilityRepository
    {
        Task<List<Ability>> SaveAbilityAsync(List<Models.Ability> abilities, CancellationToken cancellationToken = default);
        Task<List<Models.Ability>> GetAbilitiesByCardIdAsync(string cardId);
    }
}
