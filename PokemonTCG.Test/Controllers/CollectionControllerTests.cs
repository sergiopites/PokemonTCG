using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PokemonTCG.API.Controllers;
using PokemonTCG.API.Request;
using PokemonTCG.API.Services;

namespace PokemonTCG.Test.Controllers
{
    public class CollectionControllerTests
    {
        private readonly Mock<ICollectionService> _mockCollectionService;
        private readonly Mock<ILogger<CollectionController>> _mockLogger;
        private readonly CollectionController _controller;

        public CollectionControllerTests()
        {
            _mockCollectionService = new Mock<ICollectionService>();
            _mockLogger = new Mock<ILogger<CollectionController>>();
            _controller = new CollectionController(_mockCollectionService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task SaveCollection_ReturnsOk_WhenSuccessful()
        {
            var request = new CollectionRequest
            {
                Name = "My Collection",
                Description = "Test collection",
                Cards = new List<CollectionCardQuantityRequest>
                {
                    new CollectionCardQuantityRequest { CardId = "xy1-1", Quantity = 3 }
                }
            };
            _mockCollectionService.Setup(s => s.SaveCollectionAsync(It.IsAny<CollectionRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(request);

            var result = await _controller.SaveCollection(request, CancellationToken.None);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task SaveCollection_ReturnsBadRequest_WhenExceptionThrown()
        {
            var request = new CollectionRequest
            {
                Name = "My Collection",
                Description = "Test collection",
                Cards = new List<CollectionCardQuantityRequest>()
            };
            _mockCollectionService.Setup(s => s.SaveCollectionAsync(It.IsAny<CollectionRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Save failed"));

            var result = await _controller.SaveCollection(request, CancellationToken.None);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Save failed", badRequest.Value);
        }
    }
}
