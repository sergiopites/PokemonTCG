using PokemonTCG.API.Models;

namespace PokemonTCG.API.Repositories
{
    public interface ICardMarketRepository
    {
        Task<CardMarket> SaveCardMarketAsync(CardMarket cardMarket, CancellationToken cancellationToken);
    }
}
