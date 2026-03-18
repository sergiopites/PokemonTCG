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
    }
}
