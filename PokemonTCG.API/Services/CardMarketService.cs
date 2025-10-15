using PokemonTCG.API.Repositories;

namespace PokemonTCG.API.Services
{
    public class CardMarketService : ICardMarketService
    {
        private readonly ILogger<CardMarketService> _logger;
        private readonly ICardMarketRepository _cardMarketRepository;
        public CardMarketService(ILogger<CardMarketService> logger, ICardMarketRepository cardMarketRepository) 
        {
            _logger = logger;
            _cardMarketRepository = cardMarketRepository;
        }

        public async Task SaveCardMarketAsync(Models.CardMarket cardMarket, CancellationToken cancellationToken)
        {
            try
            {
                await _cardMarketRepository.SaveCardMarketAsync(cardMarket, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in SaveCardMarketAsync: {ex.Message}");
            }
        }
    }
}
