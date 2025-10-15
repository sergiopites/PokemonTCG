namespace PokemonTCG.API.Services
{
    public interface ICardMarketService
    {
        Task SaveCardMarketAsync(Models.CardMarket cardMarket, CancellationToken cancellationToken);
    }
}
