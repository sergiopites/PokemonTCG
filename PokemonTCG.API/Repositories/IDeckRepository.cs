using PokemonTCG.API.DTOs;

namespace PokemonTCG.API.Repositories
{
    public interface IDeckRepository
    {
        Task<DeckDetailDTO> SaveDeckAsync(DeckDetailDTO deckDetailDTO, CancellationToken cancellationToken = default);
    }
}
