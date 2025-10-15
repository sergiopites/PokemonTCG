using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using PokemonTCG.API.Data;
using PokemonTCG.API.Models;

namespace PokemonTCG.API.Repositories
{
    public class CardMarketRepository: ICardMarketRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ICardMarketRepository> _logger;
        
        public CardMarketRepository(AppDbContext context, ILogger<ICardMarketRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<CardMarket> SaveCardMarketAsync(CardMarket cardMarket, CancellationToken cancellationToken)
        {
            try
            {
                var existing = await _context.CardMarkets.FirstOrDefaultAsync(cardm =>
                        cardm.Url == cardMarket.Url && cardm.CardId == cardMarket.CardId);

                if (existing != null)
                    return existing; // O su Id

                _context.CardMarkets.Add(cardMarket);
                await _context.SaveChangesAsync();
                return cardMarket;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving card market with ID '{cardMarket.CardId}': {ex.Message}");
            }

            return cardMarket;
        }
    }
}
