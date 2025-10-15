using PokemonTCG.API.DTOs;
using PokemonTCG.API.Models;

namespace PokemonTCG.API.Repositories
{
    public interface ICardRepository
    {
        Task<List<Card>> GetCardsByNumberAsync(string number);
        Task<List<CardDetailDTO>> GetCardsByCardIdAsync(string id);
        Task<List<Card>> GetCardsBySuperTypeAsync(string supertype);
        Task<List<dynamic>> GetCardImageByCardId(string cardId);
        Task<List<CardDetailDTO>> GetCardsBySet(string setId);
        Task<Card> SaveCardAsync(Card card, CancellationToken cancellationToken);

    }
}