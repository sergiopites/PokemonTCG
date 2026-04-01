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

        [Fact]
        public async Task GetAllCollections_ReturnsOk_WhenSuccessful()
        {
            var collections = new List<CollectionRequest>
            {
                new CollectionRequest { CollectionId = 1, Name = "Col 1", Description = "Desc 1", Cards = new List<CollectionCardQuantityRequest>() },
                new CollectionRequest { CollectionId = 2, Name = "Col 2", Description = "Desc 2", Cards = new List<CollectionCardQuantityRequest>() }
            };
            _mockCollectionService.Setup(s => s.GetAllCollectionsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(collections);

            var result = await _controller.GetAllCollections(CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<List<CollectionRequest>>(okResult.Value);
            Assert.Equal(2, returned.Count);
        }

        [Fact]
        public async Task GetAllCollections_ReturnsBadRequest_WhenExceptionThrown()
        {
            _mockCollectionService.Setup(s => s.GetAllCollectionsAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("DB error"));

            var result = await _controller.GetAllCollections(CancellationToken.None);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("DB error", badRequest.Value);
        }

        [Fact]
        public async Task GetAllCollections_ReturnsOk_WhenEmpty()
        {
            _mockCollectionService.Setup(s => s.GetAllCollectionsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CollectionRequest>());

            var result = await _controller.GetAllCollections(CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<List<CollectionRequest>>(okResult.Value);
            Assert.Empty(returned);
        }

        [Fact]
        public async Task GetCollectionById_ReturnsOk_WhenFound()
        {
            var collection = new CollectionRequest
            {
                CollectionId = 1,
                Name = "My Collection",
                Description = "Test",
                Cards = new List<CollectionCardQuantityRequest>
                {
                    new CollectionCardQuantityRequest { CardId = "xy1-1", Quantity = 2 }
                }
            };
            _mockCollectionService.Setup(s => s.GetCollectionByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(collection);

            var result = await _controller.GetCollectionById(1, CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<CollectionRequest>(okResult.Value);
            Assert.Equal(1, returned.CollectionId);
            Assert.Single(returned.Cards);
        }

        [Fact]
        public async Task GetCollectionById_ReturnsNotFound_WhenNotFound()
        {
            _mockCollectionService.Setup(s => s.GetCollectionByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((CollectionRequest?)null);

            var result = await _controller.GetCollectionById(999, CancellationToken.None);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetCollectionById_ReturnsBadRequest_WhenExceptionThrown()
        {
            _mockCollectionService.Setup(s => s.GetCollectionByIdAsync(1, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("DB error"));

            var result = await _controller.GetCollectionById(1, CancellationToken.None);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("DB error", badRequest.Value);
        }

        [Fact]
        public async Task UpdateCollection_ReturnsOk_WhenSuccessful()
        {
            var request = new CollectionRequest
            {
                CollectionId = 1,
                Name = "Updated Collection",
                Description = "Updated desc",
                Cards = new List<CollectionCardQuantityRequest>
                {
                    new CollectionCardQuantityRequest { CardId = "xy1-1", Quantity = 5 }
                }
            };
            _mockCollectionService.Setup(s => s.SaveCollectionAsync(It.IsAny<CollectionRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(request);

            var result = await _controller.UpdateCollection(request, CancellationToken.None);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task UpdateCollection_ReturnsBadRequest_WhenExceptionThrown()
        {
            var request = new CollectionRequest
            {
                CollectionId = 1,
                Name = "Fail Update",
                Description = "Should fail",
                Cards = new List<CollectionCardQuantityRequest>()
            };
            _mockCollectionService.Setup(s => s.SaveCollectionAsync(It.IsAny<CollectionRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Update failed"));

            var result = await _controller.UpdateCollection(request, CancellationToken.None);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Update failed", badRequest.Value);
        }
    }
}
