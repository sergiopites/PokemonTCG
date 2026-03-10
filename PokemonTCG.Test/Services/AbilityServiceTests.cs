using Microsoft.Extensions.Logging;
using Moq;
using PokemonTCG.API.Models;
using PokemonTCG.API.Repositories;
using PokemonTCG.API.Services;

namespace PokemonTCG.Test.Services
{
    public class AbilityServiceTests
    {
        private readonly Mock<IAbilityRepository> _repo = new();
        private readonly Mock<ILogger<AbilityService>> _logger = new();

        private AbilityService CreateService() => new(_repo.Object, _logger.Object);

        // ?? SaveAbilityAsync ?????????????????????????????????????????????

        [Fact]
        public void SaveAbilityAsync_WithAbilities_CallsRepository()
        {
            var abilities = new List<Ability>
            {
                new() { AbilityId = 1, Name = "Blaze" }
            };

            CreateService().SaveAbilityAsync(abilities);

            _repo.Verify(r => r.SaveAbilityAsync(abilities, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void SaveAbilityAsync_NullList_DoesNotCallRepository()
        {
            CreateService().SaveAbilityAsync(null!);

            _repo.Verify(r => r.SaveAbilityAsync(It.IsAny<List<Ability>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public void SaveAbilityAsync_EmptyList_DoesNotCallRepository()
        {
            CreateService().SaveAbilityAsync(new List<Ability>());

            _repo.Verify(r => r.SaveAbilityAsync(It.IsAny<List<Ability>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        // ?? GetAbilitiesByCardIdAsync ?????????????????????????????????????

        [Fact]
        public async Task GetAbilitiesByCardIdAsync_ReturnsData()
        {
            var expected = new List<Ability> { new() { AbilityId = 1, Name = "Volt Absorb" } };
            _repo.Setup(r => r.GetAbilitiesByCardIdAsync("pika-1")).ReturnsAsync(expected);

            var result = await CreateService().GetAbilitiesByCardIdAsync("pika-1");

            Assert.Single(result);
            Assert.Equal("Volt Absorb", result[0].Name);
        }

        [Fact]
        public async Task GetAbilitiesByCardIdAsync_RepoThrows_ReturnsEmpty()
        {
            _repo.Setup(r => r.GetAbilitiesByCardIdAsync("bad"))
                 .ThrowsAsync(new Exception("DB error"));

            var result = await CreateService().GetAbilitiesByCardIdAsync("bad");

            Assert.Empty(result);
        }
    }
}
