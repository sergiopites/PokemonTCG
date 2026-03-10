using Microsoft.Extensions.Logging;
using Moq;
using PokemonTCG.API.Models;
using PokemonTCG.API.Repositories;
using PokemonTCG.API.Services;

namespace PokemonTCG.Test.Services
{
    public class AttackServiceTests
    {
        private readonly Mock<IAttackRepository> _repo = new();
        private readonly Mock<ILogger<AttackService>> _logger = new();

        private AttackService CreateService() => new(_repo.Object, _logger.Object);

        // ?? SaveAttackAsync ??????????????????????????????????????????????

        [Fact]
        public void SaveAttackAsync_WithAttacks_CallsRepository()
        {
            var attacks = new List<Attack> { new() { AttackId = 1, Name = "Thunderbolt" } };

            CreateService().SaveAttackAsync(attacks);

            _repo.Verify(r => r.SaveAttackAsync(attacks, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void SaveAttackAsync_NullList_DoesNotCallRepository()
        {
            CreateService().SaveAttackAsync(null!);

            _repo.Verify(r => r.SaveAttackAsync(It.IsAny<List<Attack>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public void SaveAttackAsync_EmptyList_DoesNotCallRepository()
        {
            CreateService().SaveAttackAsync(new List<Attack>());

            _repo.Verify(r => r.SaveAttackAsync(It.IsAny<List<Attack>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        // ?? GetAttacksByCardIdAsync ???????????????????????????????????????

        [Fact]
        public async Task GetAttacksByCardIdAsync_ReturnsData()
        {
            var expected = new List<Attack> { new() { AttackId = 1, Name = "Flamethrower" } };
            _repo.Setup(r => r.GetAttacksByCardIdAsync("char-1")).ReturnsAsync(expected);

            var result = await CreateService().GetAttacksByCardIdAsync("char-1");

            Assert.Single(result);
            Assert.Equal("Flamethrower", result[0].Name);
        }

        [Fact]
        public async Task GetAttacksByCardIdAsync_RepoThrows_ReturnsEmpty()
        {
            _repo.Setup(r => r.GetAttacksByCardIdAsync("bad"))
                 .ThrowsAsync(new Exception("DB error"));

            var result = await CreateService().GetAttacksByCardIdAsync("bad");

            Assert.Empty(result);
        }
    }
}
