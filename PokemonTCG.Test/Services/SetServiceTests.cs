using Microsoft.Extensions.Logging;
using Moq;
using PokemonTCG.API.DTOs;
using PokemonTCG.API.Repositories;
using PokemonTCG.API.Services;

namespace PokemonTCG.Test.Services
{
    public class SetServiceTests
    {
        private readonly Mock<ISetRepository> _setRepo = new();
        private readonly Mock<ILegalityRepository> _legalityRepo = new();
        private readonly Mock<ISetImageRepository> _setImageRepo = new();
        private readonly Mock<ILogger<SetService>> _logger = new();

        private SetService CreateService() =>
            new(_setRepo.Object, _logger.Object, _legalityRepo.Object, _setImageRepo.Object);

        // ?? GetAllSetsAsync ??????????????????????????????????????????????

        [Fact]
        public async Task GetAllSetsAsync_WithData_ReturnsMappedResponses()
        {
            var dtos = new List<SetDetailDTO>
            {
                new()
                {
                    SetId = "base1",
                    Name = "Base Set",
                    Series = "Base",
                    PrintedTotal = 102,
                    Total = 102,
                    PtcgoCode = "BS",
                    ReleaseDate = "1999/01/09",
                    UpdatedAt = "2020/08/14",
                    Logo = new Uri("https://logo.png"),
                    Symbol = new Uri("https://symbol.png")
                }
            };
            _setRepo.Setup(r => r.GetAllSetsAsync()).ReturnsAsync(dtos);

            var result = await CreateService().GetAllSetsAsync();

            Assert.Single(result);
            Assert.Equal("base1", result[0].SetId);
            Assert.Equal("Base Set", result[0].Name);
            Assert.Equal("Base", result[0].Series);
        }

        [Fact]
        public async Task GetAllSetsAsync_Empty_ReturnsEmpty()
        {
            _setRepo.Setup(r => r.GetAllSetsAsync()).ReturnsAsync(new List<SetDetailDTO>());

            var result = await CreateService().GetAllSetsAsync();

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllSetsAsync_RepoThrows_ReturnsEmpty()
        {
            _setRepo.Setup(r => r.GetAllSetsAsync()).ThrowsAsync(new Exception("DB error"));

            var result = await CreateService().GetAllSetsAsync();

            Assert.Empty(result);
        }

        // ?? GetSetByIdAsync ??????????????????????????????????????????????

        [Fact]
        public async Task GetSetByIdAsync_ExistingId_ReturnsMappedResponse()
        {
            var dtos = new List<SetDetailDTO>
            {
                new() { SetId = "base1", Name = "Base Set", Series = "Base" }
            };
            _setRepo.Setup(r => r.GetSetByIdAsync("base1")).ReturnsAsync(dtos);

            var result = await CreateService().GetSetByIdAsync("base1");

            Assert.Single(result);
            Assert.Equal("Base Set", result[0].Name);
        }

        [Fact]
        public async Task GetSetByIdAsync_NotFound_ReturnsEmpty()
        {
            _setRepo.Setup(r => r.GetSetByIdAsync("missing")).ReturnsAsync(new List<SetDetailDTO>());

            var result = await CreateService().GetSetByIdAsync("missing");

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetSetByIdAsync_NullResult_ReturnsEmpty()
        {
            _setRepo.Setup(r => r.GetSetByIdAsync("null")).ReturnsAsync((List<SetDetailDTO>)null!);

            var result = await CreateService().GetSetByIdAsync("null");

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetSetByIdAsync_RepoThrows_ReturnsEmpty()
        {
            _setRepo.Setup(r => r.GetSetByIdAsync("err")).ThrowsAsync(new Exception("DB error"));

            var result = await CreateService().GetSetByIdAsync("err");

            Assert.Empty(result);
        }

        // ?? GetSetByNameAsync ????????????????????????????????????????????

        [Fact]
        public async Task GetSetByNameAsync_ExistingName_ReturnsMapped()
        {
            var dtos = new List<SetDetailDTO>
            {
                new() { SetId = "base1", Name = "Base Set", Series = "Base" }
            };
            _setRepo.Setup(r => r.GetSetByNameAsync("Base Set")).ReturnsAsync(dtos);

            var result = await CreateService().GetSetByNameAsync("Base Set");

            Assert.Single(result);
        }

        [Fact]
        public async Task GetSetByNameAsync_NotFound_ReturnsEmpty()
        {
            _setRepo.Setup(r => r.GetSetByNameAsync("Unknown")).ReturnsAsync(new List<SetDetailDTO>());

            var result = await CreateService().GetSetByNameAsync("Unknown");

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetSetByNameAsync_RepoThrows_ReturnsEmpty()
        {
            _setRepo.Setup(r => r.GetSetByNameAsync("err")).ThrowsAsync(new Exception("DB error"));

            var result = await CreateService().GetSetByNameAsync("err");

            Assert.Empty(result);
        }

        // ?? GetSetBySerieAsync ???????????????????????????????????????????

        [Fact]
        public async Task GetSetBySerieAsync_ExistingSerie_ReturnsMapped()
        {
            var dtos = new List<SetDetailDTO>
            {
                new() { SetId = "base1", Name = "Base Set", Series = "Base" },
                new() { SetId = "jungle1", Name = "Jungle", Series = "Base" }
            };
            _setRepo.Setup(r => r.GetSetBySerieAsync("Base")).ReturnsAsync(dtos);

            var result = await CreateService().GetSetBySerieAsync("Base");

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetSetBySerieAsync_NotFound_ReturnsEmpty()
        {
            _setRepo.Setup(r => r.GetSetBySerieAsync("None")).ReturnsAsync(new List<SetDetailDTO>());

            var result = await CreateService().GetSetBySerieAsync("None");

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetSetBySerieAsync_RepoThrows_ReturnsEmpty()
        {
            _setRepo.Setup(r => r.GetSetBySerieAsync("err")).ThrowsAsync(new Exception("DB error"));

            var result = await CreateService().GetSetBySerieAsync("err");

            Assert.Empty(result);
        }
    }
}
