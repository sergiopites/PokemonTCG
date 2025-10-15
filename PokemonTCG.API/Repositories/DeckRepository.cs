using Microsoft.EntityFrameworkCore;
using PokemonTCG.API.Data;
using PokemonTCG.API.Models;

namespace PokemonTCG.API.Repositories
{
    public class DeckRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<IDeckRepository> _logger;
        public DeckRepository(ILogger<IDeckRepository> logger, AppDbContext context)
        {
                _context = context;
                _logger = logger;
        }

        public async Task<Deck> SaveDeckAsync(Models.Deck deck, CancellationToken cancellationToken = default)
        {
            try
            {
                var existing = await _context.Decks
                    .FirstOrDefaultAsync(d => d.DeckId == deck.DeckId, cancellationToken);

                if (existing != null)
                    return existing;

                _context.Decks.Add(deck);
                await _context.SaveChangesAsync();

                return deck;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving decks.");
                throw;
            }
        }
    }
}
