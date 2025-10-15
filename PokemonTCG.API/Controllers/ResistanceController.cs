using Microsoft.AspNetCore.Mvc;
using PokemonTCG.API.Models;
using PokemonTCG.API.Services;

namespace PokemonTCG.API.Controllers
{
    [ApiController]
    [Route("api/resistances")]
    public class ResistanceController : ControllerBase
    {
        private readonly IResistanceService _resistanceService;        
        public ResistanceController(IResistanceService resistanceService)
        {        
            _resistanceService = resistanceService; 
        }

        [HttpGet("getresistancesbycardid/{cardId}")]
        public async Task<ActionResult<List<Resistance>>> GetAbilitiesByCardId(string cardId)
        {
            var resistances = await _resistanceService.GetResistanceByCardId(cardId);
            if (resistances  == null || !resistances.Any())
            {
                return NotFound("No resistances found");
            }

            return Ok(resistances);
        }
    }
}
