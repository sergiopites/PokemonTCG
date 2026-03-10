using Microsoft.Extensions.Logging;
using Moq;
using PokemonTCG.API.Models;
using PokemonTCG.API.Repositories;

namespace PokemonTCG.Test.Repositories
{
    public class AttackRepositoryTests
    {
        private readonly Mock<ILogger<AttackRepository>> _logger = new();

        private AttackRepository CreateRepo(string dbName) =>
            new(DbContextFactory.Create(dbName), _logger.Object);

        // ?? SaveAttackAsync ??????????????????????????????????????????????

        [Fact]
        public async Task SaveAttackAsync_NewAttacks_SavesAndReturns()
        {
            var repo = CreateRepo(nameof(SaveAttackAsync_NewAttacks_SavesAndReturns));
            var attacks = new List<Attack>
            {
                new() { AttackId = 1, Name = "Thunderbolt", Damage = "90", CardId = "pikachu-1" }
            };

            var result = await repo.SaveAttackAsync(attacks);

            Assert.Single(result);
            Assert.Equal("Thunderbolt", result[0].Name);
        }

        [Fact]
        public async Task SaveAttackAsync_DuplicateAttack_SkipsInsert()
        {
            var repo = CreateRepo(nameof(SaveAttackAsync_DuplicateAttack_SkipsInsert));
            var attacks = new List<Attack>
            {
                new() { AttackId = 2, Name = "Ember", Damage = "30", CardId = "charmander-1" }
            };

            await repo.SaveAttackAsync(attacks);
            var result = await repo.SaveAttackAsync(attacks);

            Assert.Single(result);
        }

        [Fact]
        public async Task SaveAttackAsync_EmptyList_ReturnsEmpty()
        {
            var repo = CreateRepo(nameof(SaveAttackAsync_EmptyList_ReturnsEmpty));

            var result = await repo.SaveAttackAsync(new List<Attack>());

            Assert.Empty(result);
        }

        // ?? GetAttacksByCardIdAsync ???????????????????????????????????????

        [Fact]
        public async Task GetAttacksByCardIdAsync_ExistingCard_ReturnsAttacks()
        {
            var db = DbContextFactory.Create(nameof(GetAttacksByCardIdAsync_ExistingCard_ReturnsAttacks));
            db.Attacks.Add(new Attack { AttackId = 10, Name = "Flamethrower", CardId = "char-1" });
            await db.SaveChangesAsync();

            var repo = new AttackRepository(db, _logger.Object);
            var result = await repo.GetAttacksByCardIdAsync("char-1");

            Assert.Single(result);
            Assert.Equal("Flamethrower", result[0].Name);
        }

        [Fact]
        public async Task GetAttacksByCardIdAsync_NoMatch_ReturnsEmpty()
        {
            var repo = CreateRepo(nameof(GetAttacksByCardIdAsync_NoMatch_ReturnsEmpty));

            var result = await repo.GetAttacksByCardIdAsync("nonexistent");

            Assert.Empty(result);
        }
    }
}
