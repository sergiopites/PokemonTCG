using Microsoft.Extensions.Logging;
using Moq;
using PokemonTCG.API.DTOs;
using PokemonTCG.API.Repositories;
using PokemonTCG.API.Request;
using PokemonTCG.API.Services;

namespace PokemonTCG.Test.Services
{
    public class DeckServiceTests
    {
        private readonly Mock<IDeckRepository> _deckRepo = new();
        private readonly Mock<ICardRepository> _cardRepo = new();
        private readonly Mock<ILogger<IDeckRepository>> _logger = new();

        private DeckService CreateService() => new(_logger.Object, _deckRepo.Object, _cardRepo.Object);

        // ?? SaveDeckAsync ????????????????????????????????????????????????

        [Fact]
        public async Task SaveDeckAsync_ValidRequest_SavesAndReturns()
        {
            var request = new DeckRequest
            {
                Name = "My Deck",
                Description = "A test deck",
                Cards = new List<CardQuantityRequest>
                {
                    new() { CardId = "pikachu-1", Quantity = 2 }
                }
            };

            _deckRepo.Setup(r => r.SaveDeckAsync(It.IsAny<DeckDetailDTO>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((DeckDetailDTO dto, CancellationToken _) =>
                {
                    dto.DeckId = 42;
                    return dto;
                });

            var result = await CreateService().SaveDeckAsync(request);

            Assert.Equal(42, result.DeckId);
            Assert.Equal("My Deck", result.Name);
            Assert.Single(result.Cards);
        }

        [Fact]
        public async Task SaveDeckAsync_MapsCardsCorrectly()
        {
            var request = new DeckRequest
            {
                Name = "Deck",
                Description = "Desc",
                Cards = new List<CardQuantityRequest>
                {
                    new() { CardId = "char-1", Quantity = 3 },
                    new() { CardId = "squirtle-1", Quantity = 1 }
                }
            };

            _deckRepo.Setup(r => r.SaveDeckAsync(It.IsAny<DeckDetailDTO>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((DeckDetailDTO dto, CancellationToken _) => dto);

            var result = await CreateService().SaveDeckAsync(request);

            Assert.Equal(2, result.Cards.Count);
            Assert.Equal("char-1", result.Cards.First().CardId);
            Assert.Equal(3, result.Cards.First().Quantity);
        }

        [Fact]
        public async Task SaveDeckAsync_EmptyCards_ReturnsEmptyCards()
        {
            var request = new DeckRequest
            {
                Name = "Empty",
                Description = "No cards",
                Cards = new List<CardQuantityRequest>()
            };

            _deckRepo.Setup(r => r.SaveDeckAsync(It.IsAny<DeckDetailDTO>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((DeckDetailDTO dto, CancellationToken _) => dto);

            var result = await CreateService().SaveDeckAsync(request);

            Assert.Empty(result.Cards);
        }

        [Fact]
        public async Task SaveDeckAsync_PassesDeckIdFromRequest()
        {
            var request = new DeckRequest
            {
                DeckId = 99,
                Name = "Update",
                Description = "Updating",
                Cards = new List<CardQuantityRequest>()
            };

            DeckDetailDTO? capturedDto = null;
            _deckRepo.Setup(r => r.SaveDeckAsync(It.IsAny<DeckDetailDTO>(), It.IsAny<CancellationToken>()))
                .Callback<DeckDetailDTO, CancellationToken>((dto, _) => capturedDto = dto)
                .ReturnsAsync((DeckDetailDTO dto, CancellationToken _) => dto);

            await CreateService().SaveDeckAsync(request);

            Assert.NotNull(capturedDto);
            Assert.Equal(99, capturedDto!.DeckId);
        }
    }
}
