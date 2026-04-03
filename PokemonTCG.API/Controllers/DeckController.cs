using Microsoft.AspNetCore.Mvc;
using PokemonTCG.API.Request;
using PokemonTCG.API.Services;

namespace PokemonTCG.API.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class DeckController : ControllerBase
    {
        private readonly IDeckService _deckService;
        private readonly ICardService _cardService;
        private readonly ILogger<DeckController> _logger;
        public DeckController(IDeckService deckService, ICardService cardService, ILogger<DeckController> logger)
        {
            _deckService = deckService;
            _cardService = cardService;
            _logger = logger;
        }
        [HttpPost("create")]
        public async Task<IActionResult> SaveDeck(DeckRequest request, CancellationToken cancellationToken)
        {
            try
            {
                await _deckService.SaveDeckAsync(request, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in SaveDeck: {ex.Message}");
                return BadRequest(ex.Message);
            }
            return Ok();
        }
        [HttpGet("autodeck")]
        public async Task<IActionResult> AutoDeck()
        {
            try
            {
                var result = await _deckService.GenerateAutoDeckAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error generating automatic deck: {ex.Message}");
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllDecks(CancellationToken cancellationToken)
        {
            try
            {
                var decks = await _deckService.GetAllDecksAsync(cancellationToken);
                return Ok(decks);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetAllDecks: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDeckById(int id, CancellationToken cancellationToken)
        {
            try
            {
                var deck = await _deckService.GetDeckByIdAsync(id, cancellationToken);
                if (deck == null)
                    return NotFound();
                return Ok(deck);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetDeckById: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateDeck(DeckRequest request, CancellationToken cancellationToken)
        {
            try
            {
                await _deckService.SaveDeckAsync(request, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in UpdateDeck: {ex.Message}");
                return BadRequest(ex.Message);
            }
            return Ok();
        }
    }
}
