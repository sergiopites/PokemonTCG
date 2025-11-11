using PokemonTCG.API.Request;
using PokemonTCG.API.Responses;

namespace PokemonTCG.API.Services
{
    public interface IDeckService
    {
        Task<DeckRequest> SaveDeckAsync(DeckRequest deckRequest, CancellationToken cancellationToken = default);
        Task<DeckDetailResponse> GenerateAutoDeckAsync();
    }
}
