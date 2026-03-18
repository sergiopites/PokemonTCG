using Microsoft.AspNetCore.Mvc;
using PokemonTCG.API.Request;
using PokemonTCG.API.Services;

namespace PokemonTCG.API.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class CollectionController : ControllerBase
    {
        private readonly ICollectionService _collectionService;
        private readonly ILogger<CollectionController> _logger;

        public CollectionController(ICollectionService collectionService, ILogger<CollectionController> logger)
        {
            _collectionService = collectionService;
            _logger = logger;
        }

        [HttpPost("create")]
        public async Task<IActionResult> SaveCollection(CollectionRequest request, CancellationToken cancellationToken)
        {
            try
            {
                await _collectionService.SaveCollectionAsync(request, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in SaveCollection: {ex.Message}");
                return BadRequest(ex.Message);
            }
            return Ok();
        }
    }
}
