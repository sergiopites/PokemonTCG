using PokemonTCG.API.Models;

namespace PokemonTCG.API.Repositories
{
    public interface IAncientTraitRepository
    {
        Task<AncientTrait> SaveAncientTraitAsync(AncientTrait ancientTraits, CancellationToken cancellationToken = default);
    }
}
