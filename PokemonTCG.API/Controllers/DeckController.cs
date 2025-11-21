using Microsoft.AspNetCore.Mvc;
using PokemonTCG.API.DTOs;
using PokemonTCG.API.Request;
using PokemonTCG.API.Services;
using System.Text.RegularExpressions;

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
    }
}
