using PokemonTCG.API.Responses;

namespace PokemonTCG.API.Services
{
    public interface ICardService
    {
        Task<List<Models.Card>> GetCardsByNumberAsync(string number);
        Task<List<CardDetailResponse>> GetCardByCardIdAsync(string id);
        Task<List<Models.Card>> GetCardsBySuperTypeAsync(string supertype);
        Task<List<dynamic>> GetCardImageByCardId(string cardId);
        Task<List<CardDetailResponse>> GetCardsBySet(string setId);
        Task SaveCardsAsync(CancellationToken cancellationToken);
    }
}
