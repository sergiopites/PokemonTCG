using PokemonTCG.API.DTOs;
using PokemonTCG.API.Repositories;
using PokemonTCG.API.Request;

namespace PokemonTCG.API.Services
{
    public class DeckService : IDeckService
    {
        public readonly ILogger<IDeckRepository> _logger;
        public readonly IDeckRepository _deckRepository;
        public DeckService(ILogger<IDeckRepository> logger, IDeckRepository deckRepository)
        {
            _logger = logger;
            _deckRepository = deckRepository;
        }

        public async Task<DeckRequest> SaveDeckAsync(DeckRequest deckRequest, CancellationToken cancellationToken = default)
        {
            var deckDTO = new DeckDetailDTO
            {

                DeckId = deckRequest.DeckId,
                Name = deckRequest.Name,
                Description = deckRequest.Description,
                Cards = deckRequest.Cards.Select(c => new DeckCardDTO
                {
                    CardId = c.CardId,
                    Quantity = c.Quantity
                }).ToList()
            };
            deckDTO = await _deckRepository.SaveDeckAsync(deckDTO, cancellationToken);

            var result = new DeckRequest
            {
                DeckId = deckDTO.DeckId,
                Name = deckDTO.Name,
                Description = deckDTO.Description,
                Cards = deckDTO.Cards.Select(c => new CardQuantityRequest
                {
                    CardId = c.CardId,
                    Quantity = c.Quantity
                }).ToList()
            };
            return result;
        }
    }
}
