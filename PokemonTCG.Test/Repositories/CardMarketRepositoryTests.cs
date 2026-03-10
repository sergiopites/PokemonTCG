using Microsoft.Extensions.Logging;
using Moq;
using PokemonTCG.API.Models;
using PokemonTCG.API.Repositories;

namespace PokemonTCG.Test.Repositories
{
    public class CardMarketRepositoryTests
    {
        private readonly Mock<ILogger<ICardMarketRepository>> _logger = new();

        private CardMarketRepository CreateRepo(string dbName) =>
            new(DbContextFactory.Create(dbName), _logger.Object);

        // ?? SaveCardMarketAsync ??????????????????????????????????????????

        [Fact]
        public async Task SaveCardMarketAsync_NewCardMarket_SavesAndReturns()
        {
            var repo = CreateRepo(nameof(SaveCardMarketAsync_NewCardMarket_SavesAndReturns));
            var cardMarket = new CardMarket
            {
                Url = new Uri("https://cardmarket.com/pikachu"),
                CardId = "pikachu-1",
                CardMarketPrices = new List<CardMarketPrice>()
            };

            var result = await repo.SaveCardMarketAsync(cardMarket, CancellationToken.None);

            Assert.Equal("pikachu-1", result.CardId);
        }

        [Fact]
        public async Task SaveCardMarketAsync_DuplicateCardMarket_ReturnsExisting()
        {
            var db = DbContextFactory.Create(nameof(SaveCardMarketAsync_DuplicateCardMarket_ReturnsExisting));
            var url = new Uri("https://cardmarket.com/charizard");
            var existing = new CardMarket
            {
                CardMarketId = 1,
                Url = url,
                CardId = "charizard-1",
                CardMarketPrices = new List<CardMarketPrice>()
            };
            db.CardMarkets.Add(existing);
            await db.SaveChangesAsync();

            var repo = new CardMarketRepository(db, _logger.Object);
            var duplicate = new CardMarket
            {
                Url = url,
                CardId = "charizard-1",
                CardMarketPrices = new List<CardMarketPrice>()
            };

            var result = await repo.SaveCardMarketAsync(duplicate, CancellationToken.None);

            Assert.Equal(1, result.CardMarketId);
        }

        [Fact]
        public async Task SaveCardMarketAsync_NullUrl_SavesWithNullUrl()
        {
            var repo = CreateRepo(nameof(SaveCardMarketAsync_NullUrl_SavesWithNullUrl));
            var cardMarket = new CardMarket
            {
                Url = null,
                CardId = "bulbasaur-1",
                CardMarketPrices = new List<CardMarketPrice>()
            };

            var result = await repo.SaveCardMarketAsync(cardMarket, CancellationToken.None);

            Assert.Null(result.Url);
        }
    }
}
