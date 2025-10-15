namespace PokemonTCG.API.Services
{
    public interface IAncientTraitService
    {
        void SaveAntientTraitAsync(Models.AncientTrait ancientTraits, CancellationToken cancellationToken = default);
    }
}
