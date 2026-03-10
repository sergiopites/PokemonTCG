using Microsoft.Extensions.Logging;
using Moq;
using PokemonTCG.API.Models;
using PokemonTCG.API.Repositories;
using PokemonTCG.API.Services;

namespace PokemonTCG.Test.Services
{
    public class ResistanceServiceTests
    {
        private readonly Mock<IResistanceRepository> _repo = new();
        private readonly Mock<ILogger<IResistanceService>> _logger = new();

        private ResistanceService CreateService() => new(_logger.Object, _repo.Object);

        [Fact]
        public async Task GetResistanceByCardId_ReturnsData()
        {
            var expected = new List<Resistance>
            {
                new() { ResistanceId = 1, Type = "Fire", Value = "-30", CardId = "squirtle-1" }
            };
            _repo.Setup(r => r.GetResistancesByCardIdAsync("squirtle-1")).ReturnsAsync(expected);

            var result = await CreateService().GetResistanceByCardId("squirtle-1");

            Assert.Single(result);
            Assert.Equal("Fire", result[0].Type);
        }

        [Fact]
        public async Task GetResistanceByCardId_RepoThrows_ReturnsEmpty()
        {
            _repo.Setup(r => r.GetResistancesByCardIdAsync("bad"))
                 .ThrowsAsync(new Exception("DB error"));

            var result = await CreateService().GetResistanceByCardId("bad");

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetResistanceByCardId_NoResults_ReturnsEmpty()
        {
            _repo.Setup(r => r.GetResistancesByCardIdAsync("none")).ReturnsAsync(new List<Resistance>());

            var result = await CreateService().GetResistanceByCardId("none");

            Assert.Empty(result);
        }
    }
}
