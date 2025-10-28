using Microsoft.EntityFrameworkCore;
using PokemonTCG.API.Data;
using PokemonTCG.API.DTOs;
using PokemonTCG.API.Models;
using PokemonTCG.API.Request;

namespace PokemonTCG.API.Repositories
{
    public class DeckRepository : IDeckRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<IDeckRepository> _logger;
        public DeckRepository(ILogger<IDeckRepository> logger, AppDbContext context)
        {
                _context = context;
                _logger = logger;
        }

        public async Task<DeckDetailDTO> SaveDeckAsync(DeckDetailDTO deckDetailDTO, CancellationToken cancellationToken = default)
        {
            try
            {
                var deck = await _context.Decks
                    .Include(d => d.DeckCards)
                    .FirstOrDefaultAsync(d => d.DeckId == deckDetailDTO.DeckId, cancellationToken);

                if (deck == null)
                {
                    deck = new Deck
                    {
                        Name = deckDetailDTO.Name,
                        Description = deckDetailDTO.Description
                    };
                    _context.Decks.Add(deck);
                }
                else
                {
                    deck.Name = deckDetailDTO.Name;
                    deck.Description = deckDetailDTO.Description;

                    // Eliminar relaciones anteriores
                    _context.DeckCards.RemoveRange(deck.DeckCards);
                    deck.DeckCards.Clear();
                }

                // Agregar las nuevas cartas
                foreach (var cardDto in deckDetailDTO.Cards)
                {
                    int quantity = Math.Clamp(cardDto.Quantity, 1, 4); // Máximo 4 copias

                    deck.DeckCards.Add(new DeckCard
                    {
                        Deck = deck,
                        CardId = cardDto.CardId,
                        Quantity = quantity
                    });
                }

                await _context.SaveChangesAsync(cancellationToken);

                // Retornar el DTO actualizado
                deckDetailDTO.DeckId = deck.DeckId;
                return deckDetailDTO;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving deck.");
                throw;
            }
        }


    }
}
