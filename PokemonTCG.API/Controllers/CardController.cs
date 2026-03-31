using Microsoft.AspNetCore.Mvc;
using PokemonTCG.API.Responses;
using PokemonTCG.API.Services;

namespace PokemonTCG.API.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class CardController : ControllerBase
    {
        private readonly ICardService _cardService;

        public CardController(ICardService cardService)
        {
            _cardService = cardService;
        }        

        [HttpGet("cardid/{cardid}")]
        public async Task<ActionResult<List<CardDetailResponse>>> GetCardByCardId(string cardid)
        {
            var card = await _cardService.GetCardByCardIdAsync(cardid);
            if (card == null && !card.Any())
            {
                return NotFound();
            }
            return Ok(card);
        }

        [HttpGet("setid/{setid}")]
        public async Task<ActionResult<List<CardDetailResponse>>> GetCardsBySet(string setid)
        {
            var cards = await _cardService.GetCardsBySet(setid);
            if (cards == null || !cards.Any())
            {
                return NotFound();
            }

            return Ok(cards);
        }        
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string? name, [FromQuery] string? number,[FromQuery] string? setId,[FromQuery] string? supertype, [FromQuery] string? ptcgoCode,
                                                [FromQuery] string? subtype,[FromQuery] string? type,[FromQuery] string? rarity,
                                                [FromQuery] int page = 1,[FromQuery] int pageSize = 55)
        {
          var result = await _cardService.SearchCardsAsync(name: name, setId: setId, ptcgoCode: ptcgoCode,
                                                          supertype: supertype, subtype: subtype, type: type,
                                                          rarity: rarity, page: page, pageSize: pageSize, number: number);      

            return Ok(result);
        }

        [HttpGet("filters")]
        public async Task<IActionResult> GetFilters()
        {
            var rarities = await _cardService.GetDistinctRaritiesAsync();
            var types = await _cardService.GetDistinctTypesAsync();
            var supertypes = await _cardService.GetDistinctSupertypesAsync();
            var subtypes = await _cardService.GetDistinctSubtypesAsync();

            return Ok(new
            {
                Rarities = rarities,
                Types = types,
                Supertypes = supertypes,
                Subtypes = subtypes
            });
        }
        [HttpPost("addpokemoncards")]
        public async Task<IActionResult> SavePokemonCards(CancellationToken cancellationToken)
        {
            await _cardService.SaveCardsAsync(cancellationToken);
            return Ok();
        }
    }
}