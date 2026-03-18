using Microsoft.Extensions.Logging;
using Moq;
using PokemonTCG.API.DTOs;
using PokemonTCG.API.Repositories;
using PokemonTCG.API.Request;
using PokemonTCG.API.Services;

namespace PokemonTCG.Test.Services
{
    public class CollectionServiceTests
    {
        private readonly Mock<ICollectionRepository> _collectionRepo = new();
        private readonly Mock<ILogger<ICollectionRepository>> _logger = new();

        private CollectionService CreateService() => new(_logger.Object, _collectionRepo.Object);

        // ?? SaveCollectionAsync ?????????????????????????????????????????

        [Fact]
        public async Task SaveCollectionAsync_ValidRequest_SavesAndReturns()
        {
            var request = new CollectionRequest
            {
                Name = "My Collection",
                Description = "A test collection",
                Cards = new List<CollectionCardQuantityRequest>
                {
                    new() { CardId = "pikachu-1", Quantity = 3 }
                }
            };

            _collectionRepo.Setup(r => r.SaveCollectionAsync(It.IsAny<CollectionDetailDTO>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((CollectionDetailDTO dto, CancellationToken _) =>
                {
                    dto.CollectionId = 10;
                    return dto;
                });

            var result = await CreateService().SaveCollectionAsync(request);

            Assert.Equal(10, result.CollectionId);
            Assert.Equal("My Collection", result.Name);
            Assert.Equal("A test collection", result.Description);
            Assert.Single(result.Cards);
        }

        [Fact]
        public async Task SaveCollectionAsync_MapsCardsCorrectly()
        {
            var request = new CollectionRequest
            {
                Name = "Collection",
                Description = "Desc",
                Cards = new List<CollectionCardQuantityRequest>
                {
                    new() { CardId = "char-1", Quantity = 4 },
                    new() { CardId = "squirtle-1", Quantity = 1 }
                }
            };

            _collectionRepo.Setup(r => r.SaveCollectionAsync(It.IsAny<CollectionDetailDTO>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((CollectionDetailDTO dto, CancellationToken _) => dto);

            var result = await CreateService().SaveCollectionAsync(request);

            Assert.Equal(2, result.Cards.Count);
            Assert.Equal("char-1", result.Cards.First().CardId);
            Assert.Equal(4, result.Cards.First().Quantity);
            Assert.Equal("squirtle-1", result.Cards.Last().CardId);
            Assert.Equal(1, result.Cards.Last().Quantity);
        }

        [Fact]
        public async Task SaveCollectionAsync_EmptyCards_ReturnsEmptyCards()
        {
            var request = new CollectionRequest
            {
                Name = "Empty",
                Description = "No cards",
                Cards = new List<CollectionCardQuantityRequest>()
            };

            _collectionRepo.Setup(r => r.SaveCollectionAsync(It.IsAny<CollectionDetailDTO>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((CollectionDetailDTO dto, CancellationToken _) => dto);

            var result = await CreateService().SaveCollectionAsync(request);

            Assert.Empty(result.Cards);
        }

        [Fact]
        public async Task SaveCollectionAsync_PassesCollectionIdFromRequest()
        {
            var request = new CollectionRequest
            {
                CollectionId = 55,
                Name = "Update",
                Description = "Updating",
                Cards = new List<CollectionCardQuantityRequest>()
            };

            CollectionDetailDTO? capturedDto = null;
            _collectionRepo.Setup(r => r.SaveCollectionAsync(It.IsAny<CollectionDetailDTO>(), It.IsAny<CancellationToken>()))
                .Callback<CollectionDetailDTO, CancellationToken>((dto, _) => capturedDto = dto)
                .ReturnsAsync((CollectionDetailDTO dto, CancellationToken _) => dto);

            await CreateService().SaveCollectionAsync(request);

            Assert.NotNull(capturedDto);
            Assert.Equal(55, capturedDto!.CollectionId);
        }

        [Fact]
        public async Task SaveCollectionAsync_NullCollectionId_PassesNull()
        {
            var request = new CollectionRequest
            {
                CollectionId = null,
                Name = "New",
                Description = "Brand new",
                Cards = new List<CollectionCardQuantityRequest>()
            };

            CollectionDetailDTO? capturedDto = null;
            _collectionRepo.Setup(r => r.SaveCollectionAsync(It.IsAny<CollectionDetailDTO>(), It.IsAny<CancellationToken>()))
                .Callback<CollectionDetailDTO, CancellationToken>((dto, _) => capturedDto = dto)
                .ReturnsAsync((CollectionDetailDTO dto, CancellationToken _) => dto);

            await CreateService().SaveCollectionAsync(request);

            Assert.NotNull(capturedDto);
            Assert.Null(capturedDto!.CollectionId);
        }

        [Fact]
        public async Task SaveCollectionAsync_PassesCancellationToken()
        {
            var request = new CollectionRequest
            {
                Name = "Token Test",
                Description = "Test",
                Cards = new List<CollectionCardQuantityRequest>()
            };

            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            CancellationToken capturedToken = default;
            _collectionRepo.Setup(r => r.SaveCollectionAsync(It.IsAny<CollectionDetailDTO>(), It.IsAny<CancellationToken>()))
                .Callback<CollectionDetailDTO, CancellationToken>((_, ct) => capturedToken = ct)
                .ReturnsAsync((CollectionDetailDTO dto, CancellationToken _) => dto);

            await CreateService().SaveCollectionAsync(request, token);

            Assert.Equal(token, capturedToken);
        }

        [Fact]
        public async Task SaveCollectionAsync_RepositoryThrows_Propagates()
        {
            var request = new CollectionRequest
            {
                Name = "Fail",
                Description = "Should fail",
                Cards = new List<CollectionCardQuantityRequest>()
            };

            _collectionRepo.Setup(r => r.SaveCollectionAsync(It.IsAny<CollectionDetailDTO>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("DB error"));

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                CreateService().SaveCollectionAsync(request));
        }
    }
}
