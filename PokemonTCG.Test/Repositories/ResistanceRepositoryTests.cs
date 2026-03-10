using Microsoft.Extensions.Logging;
using Moq;
using PokemonTCG.API.Models;
using PokemonTCG.API.Repositories;

namespace PokemonTCG.Test.Repositories
{
    public class ResistanceRepositoryTests
    {
        private readonly Mock<ILogger<ResistanceRepository>> _logger = new();

        private ResistanceRepository CreateRepo(string dbName) =>
            new(_logger.Object, DbContextFactory.Create(dbName));

        // ?? GetResistancesByCardIdAsync ???????????????????????????????????

        [Fact]
        public async Task GetResistancesByCardIdAsync_ExistingCard_ReturnsResistances()
        {
            var db = DbContextFactory.Create(nameof(GetResistancesByCardIdAsync_ExistingCard_ReturnsResistances));
            db.Resistances.Add(new Resistance { ResistanceId = 1, Type = "Fire", Value = "-30", CardId = "squirtle-1" });
            await db.SaveChangesAsync();

            var repo = new ResistanceRepository(_logger.Object, db);
            var result = await repo.GetResistancesByCardIdAsync("squirtle-1");

            Assert.Single(result);
            Assert.Equal("Fire", result[0].Type);
        }

        [Fact]
        public async Task GetResistancesByCardIdAsync_NoMatch_ReturnsEmpty()
        {
            var repo = CreateRepo(nameof(GetResistancesByCardIdAsync_NoMatch_ReturnsEmpty));

            var result = await repo.GetResistancesByCardIdAsync("nonexistent");

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetResistancesByCardIdAsync_MultipleCards_ReturnsOnlyMatching()
        {
            var db = DbContextFactory.Create(nameof(GetResistancesByCardIdAsync_MultipleCards_ReturnsOnlyMatching));
            db.Resistances.AddRange(
                new Resistance { ResistanceId = 1, Type = "Fire", Value = "-30", CardId = "squirtle-1" },
                new Resistance { ResistanceId = 2, Type = "Water", Value = "-20", CardId = "charmander-1" }
            );
            await db.SaveChangesAsync();

            var repo = new ResistanceRepository(_logger.Object, db);
            var result = await repo.GetResistancesByCardIdAsync("squirtle-1");

            Assert.Single(result);
            Assert.Equal("squirtle-1", result[0].CardId);
        }
    }
}
