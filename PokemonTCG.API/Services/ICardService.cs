using PokemonTCG.API.DTOs;
using PokemonTCG.API.Helpers;
using PokemonTCG.API.Responses;

namespace PokemonTCG.API.Services
{
    public interface ICardService
    {
        Task<List<Models.Card>> GetCardsByNumberAsync(string number);
        Task<List<CardDetailResponse>> GetCardByCardIdAsync(string id);
        Task<List<Models.Card>> GetCardsBySuperTypeAsync(string supertype);        
        Task<List<CardDetailResponse>> GetCardsBySet(string setId);
        Task<CardDetailResponse> GetCardsByRarityAsync(string rarity, int page = 1, int pageSize = 55);
        Task<CardDetailResponse> GetCardsByTypeAsync(string type, int page = 1, int pageSize = 55);
        Task<CardDetailResponse> GetCardsBySupertypeAsync(string supertype, int page = 1, int pageSize = 55);
        Task<CardDetailResponse> GetCardsBySubtypeAsync(string subtype, int page = 1, int pageSize = 55);
        Task<CardDetailResponse> GetCardsBySetAsync(string setCode, int page = 1, int pageSize = 55);
        Task<CardDetailResponse> GetCardsByNameAsync(string name, int page = 1, int pageSize = 55);
        Task<PagedResult<CardDetailDTO>> SearchCardsAsync(string? name = null, string? setId = null, string? ptcgoCode = null,
                                                                       string? supertype = null, string? subtype = null, string? type = null,
                                                                       string? rarity = null, int page = 1, int pageSize = 55, string? number = null);
        Task<List<CardDetailResponse>> GetDistinctTypesAsync();
        Task<List<CardDetailResponse>> GetDistinctRaritiesAsync();
        Task<List<CardDetailResponse>> GetDistinctSubtypesAsync();
        Task<List<CardDetailResponse>> GetDistinctSupertypesAsync();
        Task SaveCardsAsync(CancellationToken cancellationToken);
    }
}
