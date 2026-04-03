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
                    bool isEnergy = (cardDto.Supertype ?? "").Equals("Energy", StringComparison.OrdinalIgnoreCase);
                    int quantity = isEnergy
                        ? Math.Max(cardDto.Quantity, 1)
                        : Math.Clamp(cardDto.Quantity, 1, 4);

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

        public async Task<List<DeckDetailDTO>> GetAllDecksAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var decks = await _context.Decks
                    .Include(d => d.DeckCards)
                    .ToListAsync(cancellationToken);

                return decks.Select(d => new DeckDetailDTO
                {
                    DeckId = d.DeckId,
                    Name = d.Name,
                    Description = d.Description,
                    Cards = d.DeckCards.Select(dc => new DeckCardDTO
                    {
                        CardId = dc.CardId,
                        Quantity = dc.Quantity
                    }).ToList()
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all decks.");
                throw;
            }
        }

        public async Task<DeckDetailDTO?> GetDeckByIdAsync(int deckId, CancellationToken cancellationToken = default)
        {
            try
            {
                var deck = await _context.Decks
                    .Include(d => d.DeckCards)
                        .ThenInclude(dc => dc.Card)
                            .ThenInclude(card => card.CardImage)
                    .Include(d => d.DeckCards)
                        .ThenInclude(dc => dc.Card)
                            .ThenInclude(card => card.Set)
                    .FirstOrDefaultAsync(d => d.DeckId == deckId, cancellationToken);

                if (deck == null)
                    return null;

                return new DeckDetailDTO
                {
                    DeckId = deck.DeckId,
                    Name = deck.Name,
                    Description = deck.Description,
                    Cards = deck.DeckCards.Select(dc => new DeckCardDTO
                    {
                        CardId = dc.CardId,
                        Quantity = dc.Quantity,
                        Name = dc.Card?.Name,
                        ImageLarge = dc.Card?.CardImage?.Large?.ToString(),
                        Supertype = dc.Card?.SuperType,
                        Subtype = dc.Card?.SubTypes,
                        Number = dc.Card?.Number,
                        SetName = dc.Card?.Set?.Name,
                        SetId = dc.Card?.SetId,
                        Ptcgocode = dc.Card?.Set?.PtcgoCode
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting deck by id.");
                throw;
            }
        }

    }
}
