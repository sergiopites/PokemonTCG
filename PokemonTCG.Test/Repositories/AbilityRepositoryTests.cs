using Microsoft.Extensions.Logging;
using Moq;
using PokemonTCG.API.Models;
using PokemonTCG.API.Repositories;

namespace PokemonTCG.Test.Repositories
{
    public class AbilityRepositoryTests
    {
        private readonly Mock<ILogger<AbilityRepository>> _logger = new();

        private AbilityRepository CreateRepo(string dbName) =>
            new(DbContextFactory.Create(dbName), _logger.Object);

        // ?? SaveAbilityAsync ?????????????????????????????????????????????

        [Fact]
        public async Task SaveAbilityAsync_NewAbilities_SavesAndReturns()
        {
            var repo = CreateRepo(nameof(SaveAbilityAsync_NewAbilities_SavesAndReturns));
            var abilities = new List<Ability>
            {
                new() { AbilityId = 1, Name = "Volt Absorb", Text = "Heals", Type = "Ability", CardId = "pikachu-1" }
            };

            var result = await repo.SaveAbilityAsync(abilities);

            Assert.Single(result);
            Assert.Equal("Volt Absorb", result[0].Name);
        }

        [Fact]
        public async Task SaveAbilityAsync_DuplicateAbility_SkipsInsert()
        {
            var repo = CreateRepo(nameof(SaveAbilityAsync_DuplicateAbility_SkipsInsert));
            var abilities = new List<Ability>
            {
                new() { AbilityId = 2, Name = "Flash Fire", Text = "Fire boost", Type = "Ability", CardId = "charizard-1" }
            };

            await repo.SaveAbilityAsync(abilities);
            var result = await repo.SaveAbilityAsync(abilities); // second call = duplicate

            Assert.Single(result);
        }

        [Fact]
        public async Task SaveAbilityAsync_EmptyList_ReturnsEmpty()
        {
            var repo = CreateRepo(nameof(SaveAbilityAsync_EmptyList_ReturnsEmpty));

            var result = await repo.SaveAbilityAsync(new List<Ability>());

            Assert.Empty(result);
        }

        // ?? GetAbilitiesByCardIdAsync ?????????????????????????????????????

        [Fact]
        public async Task GetAbilitiesByCardIdAsync_ExistingCard_ReturnsAbilities()
        {
            var db = DbContextFactory.Create(nameof(GetAbilitiesByCardIdAsync_ExistingCard_ReturnsAbilities));
            db.Abilities.Add(new Ability { AbilityId = 10, Name = "Blaze", CardId = "char-1", Type = "Ability" });
            await db.SaveChangesAsync();

            var repo = new AbilityRepository(db, _logger.Object);
            var result = await repo.GetAbilitiesByCardIdAsync("char-1");

            Assert.Single(result);
            Assert.Equal("Blaze", result[0].Name);
        }

        [Fact]
        public async Task GetAbilitiesByCardIdAsync_NoMatch_ReturnsEmpty()
        {
            var repo = CreateRepo(nameof(GetAbilitiesByCardIdAsync_NoMatch_ReturnsEmpty));

            var result = await repo.GetAbilitiesByCardIdAsync("nonexistent-card");

            Assert.Empty(result);
        }
    }
}
