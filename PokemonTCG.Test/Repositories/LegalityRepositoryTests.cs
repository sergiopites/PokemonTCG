using Microsoft.Extensions.Logging;
using Moq;
using PokemonTCG.API.Models;
using PokemonTCG.API.Repositories;

namespace PokemonTCG.Test.Repositories
{
    public class LegalityRepositoryTests
    {
        private readonly Mock<ILogger<LegalityRepository>> _logger = new();

        private LegalityRepository CreateRepo(string dbName) =>
            new(DbContextFactory.Create(dbName), _logger.Object);

        // ?? GetAllLegalities ?????????????????????????????????????????????

        [Fact]
        public async Task GetAllLegalities_WithData_ReturnsAll()
        {
            var db = DbContextFactory.Create(nameof(GetAllLegalities_WithData_ReturnsAll));
            db.Legalities.AddRange(
                new Legality { Standard = "Legal", Expanded = "Legal", Unlimited = "Legal" },
                new Legality { Standard = "Banned", Expanded = "Legal", Unlimited = "Legal" }
            );
            await db.SaveChangesAsync();

            var repo = new LegalityRepository(db, _logger.Object);
            var result = await repo.GetAllLegalities();

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetAllLegalities_Empty_ReturnsEmpty()
        {
            var repo = CreateRepo(nameof(GetAllLegalities_Empty_ReturnsEmpty));

            var result = await repo.GetAllLegalities();

            Assert.Empty(result);
        }

        // ?? SaveLegalityAsync ????????????????????????????????????????????

        [Fact]
        public async Task SaveLegalityAsync_NewLegality_SavesAndReturns()
        {
            var repo = CreateRepo(nameof(SaveLegalityAsync_NewLegality_SavesAndReturns));
            var legality = new Legality { Standard = "Legal", Expanded = "Legal", Unlimited = "Legal" };

            var result = await repo.SaveLegalityAsync(legality);

            Assert.Equal("Legal", result.Standard);
        }

        [Fact]
        public async Task SaveLegalityAsync_DuplicateLegality_SkipsInsert()
        {
            var db = DbContextFactory.Create(nameof(SaveLegalityAsync_DuplicateLegality_SkipsInsert));
            var legality = new Legality { LegalityId = 5, Standard = "Legal" };
            db.Legalities.Add(legality);
            await db.SaveChangesAsync();

            var repo = new LegalityRepository(db, _logger.Object);
            var result = await repo.SaveLegalityAsync(legality);

            Assert.Equal(5, result.LegalityId);
        }
    }
}
