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

        // GetAllDecks

        [Fact]
        public async Task GetAllDecks_ReturnsOk_WhenSuccessful()
        {
            var decks = new List<DeckRequest>
            {
                new() { DeckId = 1, Name = "Deck 1", Description = "D1", Cards = new List<CardQuantityRequest>() },
                new() { DeckId = 2, Name = "Deck 2", Description = "D2", Cards = new List<CardQuantityRequest>() }
            };
            _mockDeckService.Setup(s => s.GetAllDecksAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(decks);

            var result = await _controller.GetAllDecks(CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<List<DeckRequest>>(okResult.Value);
            Assert.Equal(2, returned.Count);
        }

        [Fact]
        public async Task GetAllDecks_ReturnsBadRequest_WhenExceptionThrown()
        {
            _mockDeckService.Setup(s => s.GetAllDecksAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("DB error"));

            var result = await _controller.GetAllDecks(CancellationToken.None);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("DB error", badRequest.Value);
        }

        [Fact]
        public async Task GetAllDecks_ReturnsOk_WhenEmpty()
        {
            _mockDeckService.Setup(s => s.GetAllDecksAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<DeckRequest>());

            var result = await _controller.GetAllDecks(CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<List<DeckRequest>>(okResult.Value);
            Assert.Empty(returned);
        }

        // GetDeckById

        [Fact]
        public async Task GetDeckById_ReturnsOk_WhenFound()
        {
            var deck = new DeckRequest
            {
                DeckId = 1, Name = "My Deck", Description = "Desc",
                Cards = new List<CardQuantityRequest> { new() { CardId = "xy1-1", Quantity = 4 } }
            };
            _mockDeckService.Setup(s => s.GetDeckByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(deck);

            var result = await _controller.GetDeckById(1, CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<DeckRequest>(okResult.Value);
            Assert.Equal(1, returned.DeckId);
        }

        [Fact]
        public async Task GetDeckById_ReturnsNotFound_WhenNull()
        {
            _mockDeckService.Setup(s => s.GetDeckByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((DeckRequest?)null);

            var result = await _controller.GetDeckById(999, CancellationToken.None);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetDeckById_ReturnsBadRequest_WhenExceptionThrown()
        {
            _mockDeckService.Setup(s => s.GetDeckByIdAsync(1, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("DB error"));

            var result = await _controller.GetDeckById(1, CancellationToken.None);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("DB error", badRequest.Value);
        }

        // UpdateDeck

        [Fact]
        public async Task UpdateDeck_ReturnsOk_WhenSuccessful()
        {
            var request = new DeckRequest
            {
                DeckId = 1, Name = "Updated", Description = "Upd",
                Cards = new List<CardQuantityRequest> { new() { CardId = "xy1-1", Quantity = 4 } }
            };
            _mockDeckService.Setup(s => s.SaveDeckAsync(It.IsAny<DeckRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(request);

            var result = await _controller.UpdateDeck(request, CancellationToken.None);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task UpdateDeck_ReturnsBadRequest_WhenExceptionThrown()
        {
            var request = new DeckRequest
            {
                DeckId = 1, Name = "Fail", Cards = new List<CardQuantityRequest>()
            };
            _mockDeckService.Setup(s => s.SaveDeckAsync(It.IsAny<DeckRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Update failed"));

            var result = await _controller.UpdateDeck(request, CancellationToken.None);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Update failed", badRequest.Value);
        }
    }
}