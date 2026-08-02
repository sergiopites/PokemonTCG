using Microsoft.AspNetCore.Mvc;
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

        [HttpPost("create/progress")]
        public async Task SaveSetWithProgress(CancellationToken cancellationToken)
        {
            Response.ContentType = "text/event-stream";
            Response.Headers.Append("Cache-Control", "no-cache");
            Response.Headers.Append("X-Accel-Buffering", "no");

            try
            {
                await _setService.SaveSetAsync(async (msg) =>
                {
                    var data = $"data: {{\"log\":\"{msg.Replace("\"", "'")}\"}}\n\n";
                    await Response.WriteAsync(data);
                    await Response.Body.FlushAsync();
                }, cancellationToken);

                await Response.WriteAsync("data: {\"done\":true}\n\n");
                await Response.Body.FlushAsync();
            }
            catch (Exception ex)
            {
                await Response.WriteAsync($"data: {{\"error\":\"{ex.Message.Replace("\"", "'")}\"}}\n\n");
                await Response.Body.FlushAsync();
            }
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