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

        [HttpGet("all")]
        public async Task<IActionResult> GetAllCollections(CancellationToken cancellationToken)
        {
            try
            {
                var collections = await _collectionService.GetAllCollectionsAsync(cancellationToken);
                return Ok(collections);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetAllCollections: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCollectionById(int id, CancellationToken cancellationToken)
        {
            try
            {
                var collection = await _collectionService.GetCollectionByIdAsync(id, cancellationToken);
                if (collection == null)
                    return NotFound();
                return Ok(collection);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetCollectionById: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateCollection(CollectionRequest request, CancellationToken cancellationToken)
        {
            try
            {
                await _collectionService.SaveCollectionAsync(request, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in UpdateCollection: {ex.Message}");
                return BadRequest(ex.Message);
            }
            return Ok();
        }
    }
}
