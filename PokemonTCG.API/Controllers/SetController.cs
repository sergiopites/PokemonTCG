using Microsoft.AspNetCore.Mvc;
using PokemonTCG.API.Models;
using PokemonTCG.API.Repositories;
using PokemonTCG.API.Responses;
using PokemonTCG.API.Services;

namespace PokemonTCG.API.Controllers
{
    [ApiController]
    [Route("/api/sets")]
    public class SetController : ControllerBase
    {
        private readonly  ISetService _setService;

        public SetController(ISetService setService)
        {
            _setService = setService;
        }
        [HttpPost("addset")]
        public async Task<IActionResult> SaveSet(CancellationToken cancellationToken)
        {
            await _setService.SaveSetAsync(cancellationToken);
            return Ok();
        }


        [HttpGet("getsets")]
        public async Task<ActionResult<List<SetDetailResponse>>> GetAllSets()
        {
            var cards = await _setService.GetAllSetsAsync();
            if (cards == null || !cards.Any())
            {
                return NotFound();
            }
            return Ok(cards);
        }
        [HttpGet("getsets/{name}")]
        public async Task<ActionResult<List<Set>>> GetSetByName(string name)
        {
            var sets = await _setService.GetSetByNameAsync(name);
            if (sets == null || !sets.Any())
            {
                return NotFound();
            }
            return Ok(sets);
        }
        [HttpGet("getsets/{id}")]
        public async Task<ActionResult<List<Set>>> GetSetById(string id)
        {
            var sets = await _setService.GetSetByIdAsync(id);
            if (sets == null || !sets.Any())
            {
                return NotFound();
            }
            return Ok(sets);
        }
        [HttpGet("getsets/{serie}")]
        public async Task<ActionResult<List<Set>>> GetSetBySerie(string serie)
        {
            var sets = await _setService.GetSetBySerieAsync(serie);
            if (sets == null || !sets.Any())
            {
                return NotFound();
            }
            return Ok(sets);
        }
    }
}
