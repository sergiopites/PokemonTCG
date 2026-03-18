using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PokemonTCG.API.Controllers;
using PokemonTCG.API.Request;
using PokemonTCG.API.Responses;
using PokemonTCG.API.Services;

namespace PokemonTCG.Test.Controllers
{
    public class DeckControllerTests
    {
        private readonly Mock<IDeckService> _mockDeckService;
        private readonly Mock<ICardService> _mockCardService;
        private readonly Mock<ILogger<DeckController>> _mockLogger;
        private readonly DeckController _controller;

        public DeckControllerTests()
        {
            _mockDeckService = new Mock<IDeckService>();
            _mockCardService = new Mock<ICardService>();
            _mockLogger = new Mock<ILogger<DeckController>>();
            _controller = new DeckController(_mockDeckService.Object, _mockCardService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task SaveDeck_ReturnsOk_WhenSuccessful()
        {
            var request = new DeckRequest
            {
                Name = "My Deck",
                Description = "Test",
                Cards = new List<CardQuantityRequest>
                {
                    new CardQuantityRequest { CardId = "xy1-1", Quantity = 2 }
                }
            };
            _mockDeckService.Setup(s => s.SaveDeckAsync(It.IsAny<DeckRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(request);

            var result = await _controller.SaveDeck(request, CancellationToken.None);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task SaveDeck_ReturnsBadRequest_WhenExceptionThrown()
        {
            var request = new DeckRequest
            {
                Name = "My Deck",
                Cards = new List<CardQuantityRequest>()
            };
            _mockDeckService.Setup(s => s.SaveDeckAsync(It.IsAny<DeckRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Save failed"));

            var result = await _controller.SaveDeck(request, CancellationToken.None);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Save failed", badRequest.Value);
        }

        [Fact]
        public async Task AutoDeck_ReturnsOk_WhenSuccessful()
        {
            var response = new DeckDetailResponse
            {
                DominantType = "Fire",
                Total = 60,
                Cards = new List<DeckDetailResponse.DeckAutoCardDTO>()
            };
            _mockDeckService.Setup(s => s.GenerateAutoDeckAsync()).ReturnsAsync(response);

            var result = await _controller.AutoDeck();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<DeckDetailResponse>(okResult.Value);
            Assert.Equal("Fire", returned.DominantType);
        }

        [Fact]
        public async Task AutoDeck_ReturnsBadRequest_WhenExceptionThrown()
        {
            _mockDeckService.Setup(s => s.GenerateAutoDeckAsync())
                .ThrowsAsync(new Exception("No cards"));

            var result = await _controller.AutoDeck();

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("No cards", badRequest.Value!.ToString());
        }
    }
}