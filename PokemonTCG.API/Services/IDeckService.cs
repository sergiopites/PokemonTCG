using PokemonTCG.API.Request;

namespace PokemonTCG.API.Services
{
    public interface IDeckService
    {
        Task<DeckRequest> SaveDeckAsync(DeckRequest deckRequest, CancellationToken cancellationToken = default);
    }
}
