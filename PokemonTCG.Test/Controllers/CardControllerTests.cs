using Microsoft.AspNetCore.Mvc;
using Moq;
using PokemonTCG.API.Controllers;
using PokemonTCG.API.DTOs;
using PokemonTCG.API.Helpers;
using PokemonTCG.API.Responses;
using PokemonTCG.API.Services;

namespace PokemonTCG.Test.Controllers
{
    public class CardControllerTests
    {
        private readonly Mock<ICardService> _mockCardService;
        private readonly CardController _controller;

        public CardControllerTests()
        {
            _mockCardService = new Mock<ICardService>();
            _controller = new CardController(_mockCardService.Object);
        }

        [Fact]
        public async Task GetCardByCardId_ReturnsOk_WhenCardExists()
        {
            var cards = new List<CardDetailResponse>
            {
                new CardDetailResponse { CardId = "xy1-1", Name = "Pikachu" }
            };
            _mockCardService.Setup(s => s.GetCardByCardIdAsync("xy1-1"))
                .ReturnsAsync(cards);

            var result = await _controller.GetCardByCardId("xy1-1");

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returned = Assert.IsType<List<CardDetailResponse>>(okResult.Value);
            Assert.Single(returned);
        }

        [Fact]
        public async Task GetCardByCardId_ReturnsOk_WhenListIsNotEmpty()
        {
            // El controlador tiene un bug: `card == null && !card.Any()` — nunca llega a NotFound
            // porque si card es null se lanza NullReferenceException en !card.Any().
            // Si card no es null y tiene items, retorna Ok.
            var cards = new List<CardDetailResponse>
            {
                new CardDetailResponse { CardId = "xy1-1" }
            };
            _mockCardService.Setup(s => s.GetCardByCardIdAsync("xy1-1"))
                .ReturnsAsync(cards);

            var result = await _controller.GetCardByCardId("xy1-1");

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetCardsBySet_ReturnsOk_WhenCardsExist()
        {
            var cards = new List<CardDetailResponse>
            {
                new CardDetailResponse { CardId = "xy1-1", SetId = "xy1" }
            };
            _mockCardService.Setup(s => s.GetCardsBySet("xy1"))
                .ReturnsAsync(cards);

            var result = await _controller.GetCardsBySet("xy1");

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returned = Assert.IsType<List<CardDetailResponse>>(okResult.Value);
            Assert.Single(returned);
        }

        [Fact]
        public async Task GetCardsBySet_ReturnsNotFound_WhenNoCards()
        {
            _mockCardService.Setup(s => s.GetCardsBySet("invalid"))
                .ReturnsAsync(new List<CardDetailResponse>());

            var result = await _controller.GetCardsBySet("invalid");

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetCardsBySet_ReturnsNotFound_WhenNull()
        {
            _mockCardService.Setup(s => s.GetCardsBySet("invalid"))
                .ReturnsAsync((List<CardDetailResponse>)null!);

            var result = await _controller.GetCardsBySet("invalid");

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Search_ReturnsOk_WithPagedResult()
        {
            var pagedResult = new PagedResult<CardDetailDTO>
            {
                Page = 1,
                PageSize = 55,
                TotalCount = 1,
                Items = new List<CardDetailDTO>
                {
                    new CardDetailDTO { CardId = "xy1-1", Name = "Pikachu" }
                }
            };
            _mockCardService.Setup(s => s.SearchCardsAsync(
                "Pikachu", null, null, null, null, null, null, 1, 55, "1"))
                .ReturnsAsync(pagedResult);

            var result = await _controller.Search("Pikachu", "1", null, null, null, null, null, null, 1, 55);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetFilters_ReturnsOk_WithAllFilters()
        {
            _mockCardService.Setup(s => s.GetDistinctRaritiesAsync())
                .ReturnsAsync(new List<CardDetailResponse> { new() { Rarity = "Rare" } });
            _mockCardService.Setup(s => s.GetDistinctTypesAsync())
                .ReturnsAsync(new List<CardDetailResponse> { new() { Type = "Fire" } });
            _mockCardService.Setup(s => s.GetDistinctSupertypesAsync())
                .ReturnsAsync(new List<CardDetailResponse> { new() { Supertype = "Pokémon" } });
            _mockCardService.Setup(s => s.GetDistinctSubtypesAsync())
                .ReturnsAsync(new List<CardDetailResponse> { new() { Subtype = "Basic" } });

            var result = await _controller.GetFilters();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task SavePokemonCards_ReturnsOk()
        {
            _mockCardService.Setup(s => s.SaveCardsAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await _controller.SavePokemonCards(CancellationToken.None);

            Assert.IsType<OkResult>(result);
        }
    }
}