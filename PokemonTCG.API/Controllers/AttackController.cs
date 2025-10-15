using Microsoft.AspNetCore.Mvc;
using PokemonTCG.API.Services;

namespace PokemonTCG.API.Controllers
{
    [ApiController]
    [Route("api/attacks")]
    public class AttackController : ControllerBase
    {
        private readonly IAttackService _attackService;

        public AttackController(IAttackService attackService)
        {
            _attackService = attackService;
        }

        [HttpGet("getattacksbyCardId/{cardId}")]
        public async Task<ActionResult<List<dynamic>>> GetAttacksByCardId(string cardId)
        {
            var attacks = await _attackService.GetAttacksByCardIdAsync(cardId);
            if (attacks == null || !attacks.Any())
            {
                return NotFound();
            }

            return Ok(attacks);

        }
    }
}
