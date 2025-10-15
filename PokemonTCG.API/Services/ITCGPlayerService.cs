namespace PokemonTCG.API.Services
{
    public interface ITCGPlayerService
    {
        Task SaveTCGPlayerAsync(Models.TCGPlayer tcgPlayer, CancellationToken cancellationToken);
    }
}
