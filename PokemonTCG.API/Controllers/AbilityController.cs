using Microsoft.AspNetCore.Mvc;
using PokemonTCG.API.Models;
using PokemonTCG.API.Services;

namespace PokemonTCG.API.Controllers
{
    [ApiController]
    [Route("api/abilities")]
    public class AbilityController : ControllerBase
    {
        private readonly IAbilityService _abilityService;
        public AbilityController(IAbilityService abilityService)
        {
            _abilityService = abilityService;
        }

        [HttpGet("getabilitiesbycardid/{cardId}")]
        public async Task<ActionResult<List<Ability>>> GetAbilitiesByCardId(string cardId)
        {
            var abilities = await _abilityService.GetAbilitiesByCardIdAsync(cardId);
            if (abilities == null || !abilities.Any())
            {
                return NotFound("No abilities found");
            }

            return Ok(abilities);
        }
    }
}
