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
        private readonly ILogger<DeckController> _logger;
        public DeckController(IDeckService deckService, ILogger<DeckController> logger)
        {
            _deckService = deckService;
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

    }
}
