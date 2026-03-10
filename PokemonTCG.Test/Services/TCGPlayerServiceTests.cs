using Microsoft.Extensions.Logging;
using Moq;
using PokemonTCG.API.Repositories;
using PokemonTCG.API.Services;

namespace PokemonTCG.Test.Services
{
    public class TCGPlayerServiceTests
    {
        private readonly Mock<ITCGPlayerRepository> _repo = new();
        private readonly Mock<ILogger<TCGPlayerService>> _logger = new();

        private TCGPlayerService CreateService() => new(_logger.Object, _repo.Object);

        [Fact]
        public async Task SaveTCGPlayerAsync_CompletesWithoutError()
        {
            var tcgPlayer = new API.Models.TCGPlayer
            {
                Url = new Uri("https://tcgplayer.com"),
                CardId = "pikachu-1",
                TCGPlayerPrices = new List<API.Models.TCGPlayerPrice>()
            };

            await CreateService().SaveTCGPlayerAsync(tcgPlayer, CancellationToken.None);

            // Method body is commented out in the service, so no repo calls expected
        }
    }
}
