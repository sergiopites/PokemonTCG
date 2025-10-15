using Microsoft.AspNetCore.Mvc;
using PokemonTCG.API.DTOs;
using PokemonTCG.API.Responses;
using PokemonTCG.API.Services;

namespace PokemonTCG.API.Controllers
{
    [ApiController]
    [Route("/api/cards")]
    public class CardController : ControllerBase
    {
        private readonly ICardService _cardService;

        public CardController(ICardService cardService)
        {
            _cardService = cardService;
        }

        [HttpGet("getcardsbynumber/{number}")]
        public async Task<ActionResult<List<Models.Card>>> GetCardsByNumber(string number)
        {
            var cards = await _cardService.GetCardsByNumberAsync(number);
            if (cards == null || !cards.Any())
            {
                return NotFound();
            }
            return Ok(cards);
        }

        [HttpGet("getcardsbycardid/{cardid}")]
        public async Task<ActionResult<List<CardDetailResponse>>> GetCardsByCardId(string cardid)
        {
            var card = await _cardService.GetCardByCardIdAsync(cardid);
            if (card == null && !card.Any())
            {
                return NotFound();
            }
            return Ok(card);
        }
        [HttpGet("getcardsbysupertype/{supertype}")]
        public async Task<ActionResult<List<Models.Card>>> GetCardsBySuperType(string supertype)
        {
            var cards = await _cardService.GetCardsBySuperTypeAsync(supertype);
            if (cards == null || !cards.Any())
            {
                return NotFound();
            }
            return Ok(cards);
        }

        [HttpGet("getcardswithimagesbycardid/{cardid}")]
        public async Task<ActionResult<List<Models.Card>>> GetCardsByCardImageId(string cardid)
        {
            var cards = await _cardService.GetCardImageByCardId(cardid);
            if (cards == null || !cards.Any())
            {
                return NotFound();
            }
            return Ok(cards);
        }

        [HttpGet("getcardsbysetid/{setid}")]
        public async Task<ActionResult<List<CardDetailResponse>>> GetCardsBySet(string setid)
        {
            var cards = await _cardService.GetCardsBySet(setid);
            if (cards == null || !cards.Any())
            {
                return NotFound();
            }
            
            return Ok(cards);        
        
        }
        [HttpPost("addpokemoncards")]
        public async Task<IActionResult> SavePokemonCards(CancellationToken cancellationToken)
        {
            await _cardService.SaveCardsAsync(cancellationToken);
            return Ok();
        }
    }
}