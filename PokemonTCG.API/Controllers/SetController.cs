using Microsoft.AspNetCore.Mvc;
using PokemonTCG.API.Models;
using PokemonTCG.API.Repositories;
using PokemonTCG.API.Responses;
using PokemonTCG.API.Services;

namespace PokemonTCG.API.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class SetController : ControllerBase
    {
        private readonly ISetService _setService;
                
        public SetController(ISetService setService)
        {
            _setService = setService;
        }
        [HttpPost("create")]
        public async Task<IActionResult> SaveSet(CancellationToken cancellationToken)
        {
            await _setService.SaveSetAsync(cancellationToken);
            return Ok();
        }


        [HttpGet("all")]
        public async Task<ActionResult<List<SetDetailResponse>>> GetAllSets()
        {
            var cards = await _setService.GetAllSetsAsync();
            if (cards == null || !cards.Any())
            {
                return NotFound();
            }
            return Ok(cards);
        }
        [HttpGet("name/{name}")]
        public async Task<ActionResult<List<SetDetailResponse>>> GetSetByName(string name)
        {
            var sets = await _setService.GetSetByNameAsync(name);
            if (sets == null || !sets.Any())
            {
                return NotFound();
            }
            return Ok(sets);
        }
        [HttpGet("id/{id}")]
        public async Task<ActionResult<List<SetDetailResponse>>> GetSetById(string id)
        {
            var sets = await _setService.GetSetByIdAsync(id);
            if (sets == null || !sets.Any())
            {
                return NotFound();
            }
            return Ok(sets);
        }
        [HttpGet("serie/{serie}")]
        public async Task<ActionResult<List<SetDetailResponse>>> GetSetBySerie(string serie)
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
