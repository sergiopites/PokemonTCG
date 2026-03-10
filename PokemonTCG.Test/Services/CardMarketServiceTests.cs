using Microsoft.Extensions.Logging;
using Moq;
using PokemonTCG.API.Models;
using PokemonTCG.API.Repositories;
using PokemonTCG.API.Services;

namespace PokemonTCG.Test.Services
{
    public class CardMarketServiceTests
    {
        private readonly Mock<ICardMarketRepository> _repo = new();
        private readonly Mock<ILogger<CardMarketService>> _logger = new();

        private CardMarketService CreateService() => new(_logger.Object, _repo.Object);

        [Fact]
        public async Task SaveCardMarketAsync_CallsRepository()
        {
            var cm = new CardMarket
            {
                Url = new Uri("https://cardmarket.com"),
                CardId = "pikachu-1",
                CardMarketPrices = new List<CardMarketPrice>()
            };

            await CreateService().SaveCardMarketAsync(cm, CancellationToken.None);

            _repo.Verify(r => r.SaveCardMarketAsync(cm, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task SaveCardMarketAsync_RepoThrows_DoesNotRethrow()
        {
            _repo.Setup(r => r.SaveCardMarketAsync(It.IsAny<CardMarket>(), It.IsAny<CancellationToken>()))
                 .ThrowsAsync(new Exception("DB error"));

            var cm = new CardMarket { CardId = "x", CardMarketPrices = new List<CardMarketPrice>() };

            await CreateService().SaveCardMarketAsync(cm, CancellationToken.None);

            // Should not throw - error is logged
        }
    }
}
