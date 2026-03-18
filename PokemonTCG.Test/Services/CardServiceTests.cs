using Microsoft.Extensions.Logging;
using Moq;
using PokemonTCG.API.DTOs;
using PokemonTCG.API.Helpers;
using PokemonTCG.API.Repositories;
using PokemonTCG.API.Responses;
using PokemonTCG.API.Services;

namespace PokemonTCG.Test.Services
{
    public class CardServiceTests
    {
        private readonly Mock<ICardRepository> _cardRepo = new();
        private readonly Mock<ILogger<CardService>> _logger = new();
        private readonly Mock<ISetRepository> _setRepo = new();
        private readonly Mock<ICardImageRepository> _cardImageRepo = new();
        private readonly Mock<ILegalityRepository> _legalityRepo = new();
        private readonly Mock<IAbilityRepository> _abilityRepo = new();
        private readonly Mock<IAttackRepository> _attackRepo = new();
        private readonly Mock<ITCGPlayerRepository> _tcgPlayerRepo = new();
        private readonly Mock<IAncientTraitRepository> _ancientTraitRepo = new();

        private CardService CreateService() => new(
            _cardRepo.Object,
            _logger.Object,
            _setRepo.Object,
            _cardImageRepo.Object,
            _legalityRepo.Object,
            _abilityRepo.Object,
            _attackRepo.Object,
            _tcgPlayerRepo.Object,
            _ancientTraitRepo.Object
        );

        // — SearchCardsAsync (delegates to repo) —

        [Fact]
        public async Task SearchCardsAsync_DelegatesToRepository()
        {
            var pagedResult = new PagedResult<CardDetailDTO>
            {
                Page = 1, PageSize = 10, TotalCount = 1,
                Items = new List<CardDetailDTO> { new() { CardId = "pika-1", Name = "Pikachu" } }
            };
            _cardRepo.Setup(r => r.SearchCardsAsync("Pikachu", null, null, null, null, null, null, 1, 10, null))
                     .ReturnsAsync(pagedResult);

            var result = await CreateService().SearchCardsAsync(name: "Pikachu", page: 1, pageSize: 10);

            Assert.Equal(1, result.TotalCount);
            Assert.Single(result.Items);
        }

        // — GetCardsByNameAsync —

        [Fact]
        public async Task GetCardsByNameAsync_ReturnsMappedResponse()
        {
            var pagedResult = new PagedResult<CardDetailDTO>
            {
                Page = 1, PageSize = 55, TotalCount = 2,
                Items = new List<CardDetailDTO>
                {
                    new() { CardId = "pika-1", Name = "Pikachu" },
                    new() { CardId = "pika-2", Name = "Pikachu V" }
                }
            };
            _cardRepo.Setup(r => r.SearchCardsAsync("Pikachu", null, null, null, null, null, null, 1, 55, null))
                     .ReturnsAsync(pagedResult);

            var result = await CreateService().GetCardsByNameAsync("Pikachu");

            Assert.Equal(1, result.Page);
            Assert.Equal(55, result.PageSize);
            Assert.Equal(2, result.TotalCount);
        }

        // — GetCardsByRarityAsync —

        [Fact]
        public async Task GetCardsByRarityAsync_ReturnsMappedResponse()
        {
            var pagedResult = new PagedResult<CardDetailDTO>
            {
                Page = 1, PageSize = 55, TotalCount = 5,
                Items = Enumerable.Range(1, 5).Select(i => new CardDetailDTO { CardId = $"card-{i}" })
            };
            _cardRepo.Setup(r => r.SearchCardsAsync(null, null, null, null, null, null, "Rare", 1, 55, null))
                     .ReturnsAsync(pagedResult);

            var result = await CreateService().GetCardsByRarityAsync("Rare");

            Assert.Equal(5, result.TotalCount);
        }

        // — GetCardsByTypeAsync —

        [Fact]
        public async Task GetCardsByTypeAsync_ReturnsMappedResponse()
        {
            var pagedResult = new PagedResult<CardDetailDTO>
            {
                Page = 1, PageSize = 55, TotalCount = 3,
                Items = Enumerable.Range(1, 3).Select(i => new CardDetailDTO { CardId = $"card-{i}" })
            };
            _cardRepo.Setup(r => r.SearchCardsAsync(null, null, null, null, null, "Fire", null, 1, 55, null))
                     .ReturnsAsync(pagedResult);

            var result = await CreateService().GetCardsByTypeAsync("Fire");

            Assert.Equal(3, result.TotalCount);
        }

        // — GetCardsBySupertypeAsync —

        [Fact]
        public async Task GetCardsBySupertypeAsync_ReturnsMappedResponse()
        {
            var pagedResult = new PagedResult<CardDetailDTO>
            {
                Page = 1, PageSize = 55, TotalCount = 10,
                Items = Enumerable.Range(1, 10).Select(i => new CardDetailDTO { CardId = $"card-{i}" })
            };
            _cardRepo.Setup(r => r.SearchCardsAsync(null, null, null, "Pokémon", null, null, null, 1, 55, null))
                     .ReturnsAsync(pagedResult);

            var result = await CreateService().GetCardsBySupertypeAsync("Pokémon");

            Assert.Equal(10, result.TotalCount);
        }

        // — GetCardsBySubtypeAsync —

        [Fact]
        public async Task GetCardsBySubtypeAsync_ReturnsMappedResponse()
        {
            var pagedResult = new PagedResult<CardDetailDTO>
            {
                Page = 1, PageSize = 55, TotalCount = 7,
                Items = Enumerable.Range(1, 7).Select(i => new CardDetailDTO { CardId = $"card-{i}" })
            };
            _cardRepo.Setup(r => r.SearchCardsAsync(null, null, null, null, "Basic", null, null, 1, 55, null))
                     .ReturnsAsync(pagedResult);

            var result = await CreateService().GetCardsBySubtypeAsync("Basic");

            Assert.Equal(7, result.TotalCount);
        }

        // — GetCardsBySetAsync —

        [Fact]
        public async Task GetCardsBySetAsync_ReturnsMappedResponse()
        {
            var pagedResult = new PagedResult<CardDetailDTO>
            {
                Page = 1, PageSize = 55, TotalCount = 15,
                Items = Enumerable.Range(1, 15).Select(i => new CardDetailDTO { CardId = $"card-{i}" })
            };
            _cardRepo.Setup(r => r.SearchCardsAsync(null, "base1", null, null, null, null, null, 1, 55, null))
                     .ReturnsAsync(pagedResult);

            var result = await CreateService().GetCardsBySetAsync("base1");

            Assert.Equal(15, result.TotalCount);
        }

        // — GetDistinct* —

        [Fact]
        public async Task GetDistinctTypesAsync_ReturnsMapped()
        {
            _cardRepo.Setup(r => r.GetDistinctTypesAsync())
                     .ReturnsAsync(new List<string> { "Fire", "Water" });

            var result = await CreateService().GetDistinctTypesAsync();

            Assert.Equal(2, result.Count);
            Assert.Equal("Fire", result[0].Type);
        }

        [Fact]
        public async Task GetDistinctRaritiesAsync_ReturnsMapped()
        {
            _cardRepo.Setup(r => r.GetDistinctRaritiesAsync())
                     .ReturnsAsync(new List<string> { "Rare", "Common" });

            var result = await CreateService().GetDistinctRaritiesAsync();

            Assert.Equal(2, result.Count);
            Assert.Equal("Rare", result[0].Rarity);
        }

        [Fact]
        public async Task GetDistinctSubtypesAsync_ReturnsMapped()
        {
            _cardRepo.Setup(r => r.GetDistinctSubtypesAsync())
                     .ReturnsAsync(new List<string> { "Basic", "Stage 1" });

            var result = await CreateService().GetDistinctSubtypesAsync();

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetDistinctSupertypesAsync_ReturnsMapped()
        {
            _cardRepo.Setup(r => r.GetDistinctSupertypesAsync())
                     .ReturnsAsync(new List<string> { "Pokémon", "Trainer" });

            var result = await CreateService().GetDistinctSupertypesAsync();

            Assert.Equal(2, result.Count);
        }
    }
}
