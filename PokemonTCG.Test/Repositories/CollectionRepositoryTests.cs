using Microsoft.Extensions.Logging;
using Moq;
using PokemonTCG.API.DTOs;
using PokemonTCG.API.Models;
using PokemonTCG.API.Repositories;

namespace PokemonTCG.Test.Repositories
{
    public class CollectionRepositoryTests
    {
        private readonly Mock<ILogger<ICollectionRepository>> _logger = new();

        private CollectionRepository CreateRepo(string dbName) =>
            new(_logger.Object, DbContextFactory.Create(dbName));

        // ?? SaveCollectionAsync ?????????????????????????????????????????

        [Fact]
        public async Task SaveCollectionAsync_NewCollection_SavesAndReturnsWithId()
        {
            var db = DbContextFactory.Create(nameof(SaveCollectionAsync_NewCollection_SavesAndReturnsWithId));
            db.Cards.Add(new Card { CardId = "pikachu-1", ExternalId = "pikachu-1", SetId = "base1" });
            db.Sets.Add(new Set { SetId = "base1" });
            await db.SaveChangesAsync();

            var repo = new CollectionRepository(_logger.Object, db);
            var dto = new CollectionDetailDTO
            {
                Name = "My Collection",
                Description = "Test",
                Cards = new List<CollectionCardDTO>
                {
                    new() { CardId = "pikachu-1", Quantity = 3 }
                }
            };

            var result = await repo.SaveCollectionAsync(dto);

            Assert.NotNull(result.CollectionId);
            Assert.True(result.CollectionId > 0);
            Assert.Equal("My Collection", result.Name);
            Assert.Single(result.Cards);
        }

        [Fact]
        public async Task SaveCollectionAsync_UpdateExistingCollection_UpdatesNameAndReplacesCards()
        {
            var db = DbContextFactory.Create(nameof(SaveCollectionAsync_UpdateExistingCollection_UpdatesNameAndReplacesCards));
            db.Cards.Add(new Card { CardId = "char-1", ExternalId = "char-1", SetId = "base1" });
            db.Cards.Add(new Card { CardId = "bulba-1", ExternalId = "bulba-1", SetId = "base1" });
            db.Sets.Add(new Set { SetId = "base1" });

            var collection = new Collection
            {
                Name = "Old Name",
                Description = "Old Desc",
                CollectionCards = new List<CollectionCard>
                {
                    new() { CardId = "char-1", Quantity = 1 }
                }
            };
            db.Collections.Add(collection);
            await db.SaveChangesAsync();

            var repo = new CollectionRepository(_logger.Object, db);
            var dto = new CollectionDetailDTO
            {
                CollectionId = collection.CollectionId,
                Name = "New Name",
                Description = "Updated Desc",
                Cards = new List<CollectionCardDTO>
                {
                    new() { CardId = "bulba-1", Quantity = 5 }
                }
            };

            var result = await repo.SaveCollectionAsync(dto);

            Assert.Equal("New Name", result.Name);
            Assert.Equal("Updated Desc", result.Description);
            Assert.Equal(collection.CollectionId, result.CollectionId);
        }

        [Fact]
        public async Task SaveCollectionAsync_EmptyCards_SavesCollectionWithNoCards()
        {
            var repo = CreateRepo(nameof(SaveCollectionAsync_EmptyCards_SavesCollectionWithNoCards));
            var dto = new CollectionDetailDTO
            {
                Name = "Empty Collection",
                Description = "No cards",
                Cards = new List<CollectionCardDTO>()
            };

            var result = await repo.SaveCollectionAsync(dto);

            Assert.Equal("Empty Collection", result.Name);
            Assert.Empty(result.Cards);
        }

        [Fact]
        public async Task SaveCollectionAsync_NullCollectionId_CreatesNew()
        {
            var repo = CreateRepo(nameof(SaveCollectionAsync_NullCollectionId_CreatesNew));
            var dto = new CollectionDetailDTO
            {
                CollectionId = null,
                Name = "Null Id Collection",
                Description = "Should create new",
                Cards = new List<CollectionCardDTO>()
            };

            var result = await repo.SaveCollectionAsync(dto);

            Assert.NotNull(result.CollectionId);
            Assert.True(result.CollectionId > 0);
        }

        [Fact]
        public async Task SaveCollectionAsync_NonExistentId_CreatesNew()
        {
            var repo = CreateRepo(nameof(SaveCollectionAsync_NonExistentId_CreatesNew));
            var dto = new CollectionDetailDTO
            {
                CollectionId = 9999,
                Name = "Non Existent",
                Description = "Should create new because ID not found",
                Cards = new List<CollectionCardDTO>()
            };

            var result = await repo.SaveCollectionAsync(dto);

            Assert.NotNull(result.CollectionId);
            Assert.Equal("Non Existent", result.Name);
        }

        [Fact]
        public async Task SaveCollectionAsync_DbError_LogsAndThrows()
        {
            var db = DbContextFactory.Create(nameof(SaveCollectionAsync_DbError_LogsAndThrows));
            db.Dispose();

            var repo = new CollectionRepository(_logger.Object, db);
            var dto = new CollectionDetailDTO
            {
                Name = "Fail",
                Description = "Should fail",
                Cards = new List<CollectionCardDTO>()
            };

            await Assert.ThrowsAsync<ObjectDisposedException>(() => repo.SaveCollectionAsync(dto));
        }

        // ?? GetAllCollectionsAsync ?????????????????????????????????????????

        [Fact]
        public async Task GetAllCollectionsAsync_ReturnsAllCollections()
        {
            var db = DbContextFactory.Create(nameof(GetAllCollectionsAsync_ReturnsAllCollections));
            db.Sets.Add(new Set { SetId = "base1" });
            db.Cards.Add(new Card { CardId = "pikachu-1", ExternalId = "pikachu-1", SetId = "base1" });
            db.Collections.Add(new Collection
            {
                Name = "Col 1",
                Description = "Desc 1",
                CollectionCards = new List<CollectionCard>
                {
                    new() { CardId = "pikachu-1", Quantity = 2 }
                }
            });
            db.Collections.Add(new Collection
            {
                Name = "Col 2",
                Description = "Desc 2",
                CollectionCards = new List<CollectionCard>()
            });
            await db.SaveChangesAsync();

            var repo = new CollectionRepository(_logger.Object, db);
            var result = await repo.GetAllCollectionsAsync();

            Assert.Equal(2, result.Count);
            Assert.Contains(result, c => c.Name == "Col 1");
            Assert.Contains(result, c => c.Name == "Col 2");
        }

        [Fact]
        public async Task GetAllCollectionsAsync_EmptyDb_ReturnsEmptyList()
        {
            var repo = CreateRepo(nameof(GetAllCollectionsAsync_EmptyDb_ReturnsEmptyList));

            var result = await repo.GetAllCollectionsAsync();

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllCollectionsAsync_MapsCardsCorrectly()
        {
            var db = DbContextFactory.Create(nameof(GetAllCollectionsAsync_MapsCardsCorrectly));
            db.Sets.Add(new Set { SetId = "base1" });
            db.Cards.Add(new Card { CardId = "pikachu-1", ExternalId = "pikachu-1", SetId = "base1" });
            db.Cards.Add(new Card { CardId = "char-1", ExternalId = "char-1", SetId = "base1" });
            db.Collections.Add(new Collection
            {
                Name = "With Cards",
                Description = "Has cards",
                CollectionCards = new List<CollectionCard>
                {
                    new() { CardId = "pikachu-1", Quantity = 3 },
                    new() { CardId = "char-1", Quantity = 1 }
                }
            });
            await db.SaveChangesAsync();

            var repo = new CollectionRepository(_logger.Object, db);
            var result = await repo.GetAllCollectionsAsync();

            Assert.Single(result);
            Assert.Equal(2, result[0].Cards.Count);
        }

        [Fact]
        public async Task GetAllCollectionsAsync_DbError_LogsAndThrows()
        {
            var db = DbContextFactory.Create(nameof(GetAllCollectionsAsync_DbError_LogsAndThrows));
            db.Dispose();

            var repo = new CollectionRepository(_logger.Object, db);

            await Assert.ThrowsAsync<ObjectDisposedException>(() => repo.GetAllCollectionsAsync());
        }

        // ?? GetCollectionByIdAsync ?????????????????????????????????????????

        [Fact]
        public async Task GetCollectionByIdAsync_ReturnsCollection_WhenFound()
        {
            var db = DbContextFactory.Create(nameof(GetCollectionByIdAsync_ReturnsCollection_WhenFound));
            db.Sets.Add(new Set { SetId = "base1", Name = "Base Set", PtcgoCode = "BS" });
            db.Cards.Add(new Card
            {
                CardId = "pikachu-1",
                ExternalId = "pikachu-1",
                SetId = "base1",
                Name = "Pikachu",
                SuperType = "Pokémon",
                SubTypes = "Basic",
                Number = "58",
                CardImage = new CardImage { Large = new Uri("https://example.com/pikachu-large.png") }
            });
            await db.SaveChangesAsync();

            var collection = new Collection
            {
                Name = "My Col",
                Description = "Test",
                CollectionCards = new List<CollectionCard>
                {
                    new() { CardId = "pikachu-1", Quantity = 2 }
                }
            };
            db.Collections.Add(collection);
            await db.SaveChangesAsync();

            var repo = new CollectionRepository(_logger.Object, db);
            var result = await repo.GetCollectionByIdAsync(collection.CollectionId);

            Assert.NotNull(result);
            Assert.Equal(collection.CollectionId, result!.CollectionId);
            Assert.Equal("My Col", result.Name);
            Assert.Equal("Test", result.Description);
            Assert.Single(result.Cards);
            Assert.Equal("pikachu-1", result.Cards[0].CardId);
            Assert.Equal(2, result.Cards[0].Quantity);
            Assert.Equal("Pikachu", result.Cards[0].Name);
            Assert.Equal("https://example.com/pikachu-large.png", result.Cards[0].ImageLarge);
            Assert.Equal("Pokémon", result.Cards[0].Supertype);
            Assert.Equal("Basic", result.Cards[0].Subtype);
            Assert.Equal("58", result.Cards[0].Number);
            Assert.Equal("Base Set", result.Cards[0].SetName);
            Assert.Equal("base1", result.Cards[0].SetId);
            Assert.Equal("BS", result.Cards[0].Ptcgocode);
        }

        [Fact]
        public async Task GetCollectionByIdAsync_ReturnsNull_WhenNotFound()
        {
            var repo = CreateRepo(nameof(GetCollectionByIdAsync_ReturnsNull_WhenNotFound));

            var result = await repo.GetCollectionByIdAsync(9999);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetCollectionByIdAsync_EmptyCards_ReturnsEmptyCardsList()
        {
            var db = DbContextFactory.Create(nameof(GetCollectionByIdAsync_EmptyCards_ReturnsEmptyCardsList));
            var collection = new Collection
            {
                Name = "Empty",
                Description = "No cards",
                CollectionCards = new List<CollectionCard>()
            };
            db.Collections.Add(collection);
            await db.SaveChangesAsync();

            var repo = new CollectionRepository(_logger.Object, db);
            var result = await repo.GetCollectionByIdAsync(collection.CollectionId);

            Assert.NotNull(result);
            Assert.Empty(result!.Cards);
        }

        [Fact]
        public async Task GetCollectionByIdAsync_MapsMultipleCards()
        {
            var db = DbContextFactory.Create(nameof(GetCollectionByIdAsync_MapsMultipleCards));
            db.Sets.Add(new Set { SetId = "base1", Name = "Base Set", PtcgoCode = "BS" });
            db.Cards.Add(new Card
            {
                CardId = "pikachu-1",
                ExternalId = "pikachu-1",
                SetId = "base1",
                Name = "Pikachu",
                SuperType = "Pokémon",
                SubTypes = "Basic",
                Number = "58"
            });
            db.Cards.Add(new Card
            {
                CardId = "char-1",
                ExternalId = "char-1",
                SetId = "base1",
                Name = "Charizard",
                SuperType = "Pokémon",
                SubTypes = "Stage 2",
                Number = "4"
            });

            var collection = new Collection
            {
                Name = "Multi",
                Description = "Multiple cards",
                CollectionCards = new List<CollectionCard>
                {
                    new() { CardId = "pikachu-1", Quantity = 4 },
                    new() { CardId = "char-1", Quantity = 1 }
                }
            };
            db.Collections.Add(collection);
            await db.SaveChangesAsync();

            var repo = new CollectionRepository(_logger.Object, db);
            var result = await repo.GetCollectionByIdAsync(collection.CollectionId);

            Assert.NotNull(result);
            Assert.Equal(2, result!.Cards.Count);
            var pikachu = result.Cards.First(c => c.CardId == "pikachu-1");
            Assert.Equal("Pikachu", pikachu.Name);
            Assert.Equal("base1", pikachu.SetId);
            var charizard = result.Cards.First(c => c.CardId == "char-1");
            Assert.Equal("Charizard", charizard.Name);
        }

        [Fact]
        public async Task GetCollectionByIdAsync_DbError_LogsAndThrows()
        {
            var db = DbContextFactory.Create(nameof(GetCollectionByIdAsync_DbError_LogsAndThrows));
            db.Dispose();

            var repo = new CollectionRepository(_logger.Object, db);

            await Assert.ThrowsAsync<ObjectDisposedException>(() => repo.GetCollectionByIdAsync(1));
        }
    }
}
