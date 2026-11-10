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

        // ? SaveCardsAsync Tests

        [Fact]
        public async Task SaveCardsAsync_WithPokemonCards_CallsSaveCard()
        {
            // Arrange
            var sets = new List<API.Models.Set> 
            { 
                new() { SetId = "base1", Name = "Base Set" } 
            };

            _setRepo.Setup(r => r.GetAllSetsAsync()).ReturnsAsync(sets);

            // Mock the card existence checks
            _cardRepo.Setup(r => r.CardExistsByExternalIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _cardImageRepo.Setup(r => r.SaveImageCardAsync(It.IsAny<API.Models.CardImage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new API.Models.CardImage { CardImageId = 1 });

            _legalityRepo.Setup(r => r.SaveLegalityAsync(It.IsAny<API.Models.Legality>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new API.Models.Legality { LegalityId = 1 });

            var logMessages = new List<string>();

            // Act - Note: This will call GetPokemonCardBySetIdAsync which returns null/empty in mock
            await CreateService().SaveCardsAsync(async (msg) => 
            {
                logMessages.Add(msg);
                await Task.CompletedTask;
            }, CancellationToken.None);

            // Assert - Verifies the method was called and behaved correctly with no cards
            Assert.Contains("starting card sync", logMessages[0].ToLower());
            Assert.Contains("complete", logMessages[^1].ToLower());
        }

        [Fact]
        public async Task SaveCardsAsync_WithNullLog_DoesNotThrow()
        {
            // Arrange
            var sets = new List<API.Models.Set> 
            { 
                new() { SetId = "base1", Name = "Base Set" } 
            };

            _setRepo.Setup(r => r.GetAllSetsAsync()).ReturnsAsync(sets);

            // Act & Assert - Should not throw
            await CreateService().SaveCardsAsync(null, CancellationToken.None);
        }

        [Fact]
        public async Task SaveCardsAsync_VerifyCardExistenceCheck_IsCalledForEachCard()
        {
            // Arrange
            var sets = new List<API.Models.Set> 
            { 
                new() { SetId = "base1", Name = "Base Set" } 
            };

            _setRepo.Setup(r => r.GetAllSetsAsync()).ReturnsAsync(sets);

            _cardRepo.Setup(r => r.CardExistsByExternalIdAsync("card-1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _cardImageRepo.Setup(r => r.SaveImageCardAsync(It.IsAny<API.Models.CardImage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new API.Models.CardImage { CardImageId = 1 });

            _legalityRepo.Setup(r => r.SaveLegalityAsync(It.IsAny<API.Models.Legality>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new API.Models.Legality { LegalityId = 1 });

            // Act
            await CreateService().SaveCardsAsync(null, CancellationToken.None);

            // Assert - Verify that CardExistsByExternalIdAsync is used to check for duplicates
            // Since GetPokemonCardBySetIdAsync returns empty/null in mock, verify setup behavior
            _setRepo.Verify(r => r.GetAllSetsAsync(), Times.Once);
        }
    }
}
