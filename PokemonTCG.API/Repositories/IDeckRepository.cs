using PokemonTCG.API.DTOs;

namespace PokemonTCG.API.Repositories
{
    public interface IDeckRepository
    {
        Task<DeckDetailDTO> SaveDeckAsync(DeckDetailDTO deckDetailDTO, CancellationToken cancellationToken = default);
        Task<List<DeckDetailDTO>> GetAllDecksAsync(CancellationToken cancellationToken = default);
        Task<DeckDetailDTO?> GetDeckByIdAsync(int deckId, CancellationToken cancellationToken = default);
    }
}
