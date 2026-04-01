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

        // ?? GetAllCollectionsAsync ?????????????????????????????????????????

        [Fact]
        public async Task GetAllCollectionsAsync_ReturnsAllCollections()
        {
            var dtos = new List<CollectionDetailDTO>
            {
                new() { CollectionId = 1, Name = "Col 1", Description = "Desc 1", Cards = new List<CollectionCardDTO> { new() { CardId = "c1", Quantity = 2 } } },
                new() { CollectionId = 2, Name = "Col 2", Description = "Desc 2", Cards = new List<CollectionCardDTO>() }
            };

            _collectionRepo.Setup(r => r.GetAllCollectionsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(dtos);

            var result = await CreateService().GetAllCollectionsAsync();

            Assert.Equal(2, result.Count);
            Assert.Equal("Col 1", result[0].Name);
            Assert.Equal(1, result[0].CollectionId);
            Assert.Single(result[0].Cards);
            Assert.Equal("Col 2", result[1].Name);
            Assert.Empty(result[1].Cards);
        }

        [Fact]
        public async Task GetAllCollectionsAsync_EmptyList_ReturnsEmpty()
        {
            _collectionRepo.Setup(r => r.GetAllCollectionsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CollectionDetailDTO>());

            var result = await CreateService().GetAllCollectionsAsync();

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllCollectionsAsync_MapsCardsCorrectly()
        {
            var dtos = new List<CollectionDetailDTO>
            {
                new()
                {
                    CollectionId = 1, Name = "Col", Description = "D",
                    Cards = new List<CollectionCardDTO>
                    {
                        new() { CardId = "pikachu-1", Quantity = 3 },
                        new() { CardId = "char-1", Quantity = 1 }
                    }
                }
            };

            _collectionRepo.Setup(r => r.GetAllCollectionsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(dtos);

            var result = await CreateService().GetAllCollectionsAsync();

            Assert.Equal(2, result[0].Cards.Count);
            Assert.Equal("pikachu-1", result[0].Cards.First().CardId);
            Assert.Equal(3, result[0].Cards.First().Quantity);
        }

        [Fact]
        public async Task GetAllCollectionsAsync_PassesCancellationToken()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            CancellationToken capturedToken = default;
            _collectionRepo.Setup(r => r.GetAllCollectionsAsync(It.IsAny<CancellationToken>()))
                .Callback<CancellationToken>((ct) => capturedToken = ct)
                .ReturnsAsync(new List<CollectionDetailDTO>());

            await CreateService().GetAllCollectionsAsync(token);

            Assert.Equal(token, capturedToken);
        }

        [Fact]
        public async Task GetAllCollectionsAsync_RepositoryThrows_Propagates()
        {
            _collectionRepo.Setup(r => r.GetAllCollectionsAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("DB error"));

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                CreateService().GetAllCollectionsAsync());
        }

        // ?? GetCollectionByIdAsync ?????????????????????????????????????????

        [Fact]
        public async Task GetCollectionByIdAsync_ReturnsCollection_WhenFound()
        {
            var dto = new CollectionDetailDTO
            {
                CollectionId = 1,
                Name = "My Col",
                Description = "Desc",
                Cards = new List<CollectionCardDTO>
                {
                    new() { CardId = "xy1-1", Quantity = 2, Name = "Pikachu", ImageLarge = "https://img.com/pika.png", Supertype = "Pokémon", Subtype = "Basic", Number = "58", SetName = "XY", SetId = "xy1", Ptcgocode = "XY" }
                }
            };

            _collectionRepo.Setup(r => r.GetCollectionByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            var result = await CreateService().GetCollectionByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal(1, result!.CollectionId);
            Assert.Equal("My Col", result.Name);
            Assert.Equal("Desc", result.Description);
            Assert.Single(result.Cards);
            var card = result.Cards.First();
            Assert.Equal("xy1-1", card.CardId);
            Assert.Equal("Pikachu", card.Name);
            Assert.Equal("https://img.com/pika.png", card.ImageLarge);
            Assert.Equal("Pokémon", card.Supertype);
            Assert.Equal("Basic", card.Subtype);
            Assert.Equal("58", card.Number);
            Assert.Equal("XY", card.SetName);
            Assert.Equal("xy1", card.SetId);
            Assert.Equal("XY", card.Ptcgocode);
        }

        [Fact]
        public async Task GetCollectionByIdAsync_ReturnsNull_WhenNotFound()
        {
            _collectionRepo.Setup(r => r.GetCollectionByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((CollectionDetailDTO?)null);

            var result = await CreateService().GetCollectionByIdAsync(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetCollectionByIdAsync_MapsCardsCorrectly()
        {
            var dto = new CollectionDetailDTO
            {
                CollectionId = 5,
                Name = "Cards Col",
                Description = "With cards",
                Cards = new List<CollectionCardDTO>
                {
                    new() { CardId = "pikachu-1", Quantity = 4, Name = "Pikachu", ImageLarge = "https://img.com/pika.png", Supertype = "Pokémon", Subtype = "Basic", Number = "58", SetName = "Base", SetId = "base1", Ptcgocode = "BS" },
                    new() { CardId = "bulba-1", Quantity = 1, Name = "Bulbasaur", ImageLarge = "https://img.com/bulba.png", Supertype = "Pokémon", Subtype = "Basic", Number = "44", SetName = "Base", SetId = "base1", Ptcgocode = "BS" }
                }
            };

            _collectionRepo.Setup(r => r.GetCollectionByIdAsync(5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            var result = await CreateService().GetCollectionByIdAsync(5);

            Assert.NotNull(result);
            Assert.Equal(2, result!.Cards.Count);
            var first = result.Cards.First();
            Assert.Equal("pikachu-1", first.CardId);
            Assert.Equal(4, first.Quantity);
            Assert.Equal("Pikachu", first.Name);
            Assert.Equal("https://img.com/pika.png", first.ImageLarge);
            Assert.Equal("Pokémon", first.Supertype);
            Assert.Equal("58", first.Number);
            Assert.Equal("BS", first.Ptcgocode);
            var last = result.Cards.Last();
            Assert.Equal("bulba-1", last.CardId);
            Assert.Equal(1, last.Quantity);
            Assert.Equal("Bulbasaur", last.Name);
        }

        [Fact]
        public async Task GetCollectionByIdAsync_PassesCancellationToken()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            CancellationToken capturedToken = default;
            _collectionRepo.Setup(r => r.GetCollectionByIdAsync(1, It.IsAny<CancellationToken>()))
                .Callback<int, CancellationToken>((_, ct) => capturedToken = ct)
                .ReturnsAsync((CollectionDetailDTO?)null);

            await CreateService().GetCollectionByIdAsync(1, token);

            Assert.Equal(token, capturedToken);
        }

        [Fact]
        public async Task GetCollectionByIdAsync_RepositoryThrows_Propagates()
        {
            _collectionRepo.Setup(r => r.GetCollectionByIdAsync(1, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("DB error"));

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                CreateService().GetCollectionByIdAsync(1));
        }

        [Fact]
        public async Task GetCollectionByIdAsync_EmptyCards_ReturnsEmptyCardsList()
        {
            var dto = new CollectionDetailDTO
            {
                CollectionId = 10,
                Name = "Empty",
                Description = "No cards",
                Cards = new List<CollectionCardDTO>()
            };

            _collectionRepo.Setup(r => r.GetCollectionByIdAsync(10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            var result = await CreateService().GetCollectionByIdAsync(10);

            Assert.NotNull(result);
            Assert.Empty(result!.Cards);
        }
    }
}
