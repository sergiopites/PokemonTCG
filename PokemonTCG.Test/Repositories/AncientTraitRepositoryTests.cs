using Microsoft.Extensions.Logging;
using Moq;
using PokemonTCG.API.Models;
using PokemonTCG.API.Repositories;

namespace PokemonTCG.Test.Repositories
{
    public class AncientTraitRepositoryTests
    {
        private readonly Mock<ILogger<AncientTraitRepository>> _logger = new();

        private AncientTraitRepository CreateRepo(string dbName) =>
            new(DbContextFactory.Create(dbName), _logger.Object);

        // ?? SaveAncientTraitAsync ????????????????????????????????????????

        [Fact]
        public async Task SaveAncientTraitAsync_NewTrait_SavesAndReturns()
        {
            var repo = CreateRepo(nameof(SaveAncientTraitAsync_NewTrait_SavesAndReturns));
            var trait = new AncientTrait { Name = "? Stop", Text = "Prevents effects" };

            var result = await repo.SaveAncientTraitAsync(trait);

            Assert.Equal("? Stop", result.Name);
        }

        [Fact]
        public async Task SaveAncientTraitAsync_DuplicateTrait_ReturnsExisting()
        {
            var db = DbContextFactory.Create(nameof(SaveAncientTraitAsync_DuplicateTrait_ReturnsExisting));
            var existing = new AncientTrait { AncientTraitId = 1, Name = "? Double", Text = "Two attacks" };
            db.AncientTraits.Add(existing);
            await db.SaveChangesAsync();

            var repo = new AncientTraitRepository(db, _logger.Object);
            var duplicate = new AncientTrait { Name = "? Double", Text = "Two attacks" };

            var result = await repo.SaveAncientTraitAsync(duplicate);

            Assert.Equal(1, result.AncientTraitId);
        }

        [Fact]
        public async Task SaveAncientTraitAsync_NullNameAndText_SkipsSave()
        {
            var repo = CreateRepo(nameof(SaveAncientTraitAsync_NullNameAndText_SkipsSave));
            var trait = new AncientTrait { Name = null, Text = null };

            var result = await repo.SaveAncientTraitAsync(trait);

            Assert.Null(result.Name);
            Assert.Null(result.Text);
        }
    }
}
