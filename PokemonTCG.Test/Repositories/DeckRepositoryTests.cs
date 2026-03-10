using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using PokemonTCG.API.DTOs;
using PokemonTCG.API.Models;
using PokemonTCG.API.Repositories;

namespace PokemonTCG.Test.Repositories
{
    public class DeckRepositoryTests
    {
        private readonly Mock<ILogger<IDeckRepository>> _logger = new();

        private DeckRepository CreateRepo(string dbName) =>
            new(_logger.Object, DbContextFactory.Create(dbName));

        // ?? SaveDeckAsync ????????????????????????????????????????????????

        [Fact]
        public async Task SaveDeckAsync_NewDeck_SavesAndReturnsWithId()
        {
            var db = DbContextFactory.Create(nameof(SaveDeckAsync_NewDeck_SavesAndReturnsWithId));
            // Seed the card that the deck will reference
            db.Cards.Add(new Card { CardId = "pikachu-1", ExternalId = "pikachu-1", SetId = "base1" });
            db.Sets.Add(new Set { SetId = "base1" });
            await db.SaveChangesAsync();

            var repo = new DeckRepository(_logger.Object, db);
            var dto = new DeckDetailDTO
            {
                Name = "My Deck",
                Description = "Test",
                Cards = new List<DeckCardDTO>
                {
                    new() { CardId = "pikachu-1", Quantity = 2 }
                }
            };

            var result = await repo.SaveDeckAsync(dto);

            Assert.NotNull(result.DeckId);
            Assert.Equal("My Deck", result.Name);
        }

        [Fact]
        public async Task SaveDeckAsync_UpdateExistingDeck_UpdatesName()
        {
            var db = DbContextFactory.Create(nameof(SaveDeckAsync_UpdateExistingDeck_UpdatesName));
            db.Cards.Add(new Card { CardId = "char-1", ExternalId = "char-1", SetId = "base1" });
            db.Sets.Add(new Set { SetId = "base1" });
            var deck = new Deck { Name = "Old Name", Description = "Old", DeckCards = new List<DeckCard>() };
            db.Decks.Add(deck);
            await db.SaveChangesAsync();

            var repo = new DeckRepository(_logger.Object, db);
            var dto = new DeckDetailDTO
            {
                DeckId = deck.DeckId,
                Name = "New Name",
                Description = "Updated",
                Cards = new List<DeckCardDTO>
                {
                    new() { CardId = "char-1", Quantity = 1 }
                }
            };

            var result = await repo.SaveDeckAsync(dto);

            Assert.Equal("New Name", result.Name);
        }

        [Fact]
        public async Task SaveDeckAsync_QuantityExceedsFour_ClampsToFour()
        {
            var db = DbContextFactory.Create(nameof(SaveDeckAsync_QuantityExceedsFour_ClampsToFour));
            db.Cards.Add(new Card { CardId = "bulba-1", ExternalId = "bulba-1", SetId = "base1" });
            db.Sets.Add(new Set { SetId = "base1" });
            await db.SaveChangesAsync();

            var repo = new DeckRepository(_logger.Object, db);
            var dto = new DeckDetailDTO
            {
                Name = "Deck",
                Description = "Desc",
                Cards = new List<DeckCardDTO>
                {
                    new() { CardId = "bulba-1", Quantity = 10 }
                }
            };

            var result = await repo.SaveDeckAsync(dto);
            var savedDeck = db.Decks.Include(d => d.DeckCards).First(d => d.DeckId == result.DeckId);

            Assert.Equal(4, savedDeck.DeckCards.First().Quantity);
        }

        [Fact]
        public async Task SaveDeckAsync_EmptyCards_SavesDeckWithNoCards()
        {
            var (repo, _) = (CreateRepo(nameof(SaveDeckAsync_EmptyCards_SavesDeckWithNoCards)),
                             DbContextFactory.Create(nameof(SaveDeckAsync_EmptyCards_SavesDeckWithNoCards)));
            var dto = new DeckDetailDTO
            {
                Name = "Empty Deck",
                Description = "No cards",
                Cards = new List<DeckCardDTO>()
            };

            var result = await CreateRepo(nameof(SaveDeckAsync_EmptyCards_SavesDeckWithNoCards) + "2").SaveDeckAsync(dto);

            Assert.Equal("Empty Deck", result.Name);
        }
    }
}
