using PokemonTCG.API.DTOs;
using PokemonTCG.API.Helpers;
using PokemonTCG.API.Models;

namespace PokemonTCG.API.Repositories
{
    public interface ICardRepository
    {        
        Task<List<CardDetailDTO>> GetCardsByCardIdAsync(string id);        
        Task<List<CardDetailDTO>> GetCardsBySet(string setId);
        Task<PagedResult<CardDetailDTO>> SearchCardsAsync(string? name = null, string? setId = null, string? ptcgoCode = null,
                                                                       string? supertype = null, string? subtype = null, string? type = null,
                                                                       string? rarity = null, int page = 1, int pageSize = 55, string? number = null);
        Task<Card> SaveCardAsync(Card card, CancellationToken cancellationToken);
        Task<IEnumerable<string>> GetDistinctRaritiesAsync();
        Task<IEnumerable<string>> GetDistinctTypesAsync();
        Task<IEnumerable<string>> GetDistinctSupertypesAsync();
        Task<IEnumerable<string>> GetDistinctSubtypesAsync();
    }
}