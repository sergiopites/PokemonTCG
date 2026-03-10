using Microsoft.Extensions.Logging;
using Moq;
using PokemonTCG.API.Models;
using PokemonTCG.API.Repositories;

namespace PokemonTCG.Test.Repositories
{
    public class CardImageRepositoryTests
    {
        private readonly Mock<ILogger<CardImageRepository>> _logger = new();

        private CardImageRepository CreateRepo(string dbName) =>
            new(DbContextFactory.Create(dbName), _logger.Object);

        // ?? SaveImageCardAsync ???????????????????????????????????????????

        [Fact]
        public async Task SaveImageCardAsync_NewImage_SavesAndReturns()
        {
            var repo = CreateRepo(nameof(SaveImageCardAsync_NewImage_SavesAndReturns));
            var smallUri = new Uri("https://small.png");
            var largeUri = new Uri("https://large.png");
            var image = new CardImage { Small = smallUri, Large = largeUri };

            var result = await repo.SaveImageCardAsync(image, CancellationToken.None);

            Assert.Equal(smallUri, result.Small);
            Assert.Equal(largeUri, result.Large);
        }

        [Fact]
        public async Task SaveImageCardAsync_DuplicateImage_ReturnsExisting()
        {
            var db = DbContextFactory.Create(nameof(SaveImageCardAsync_DuplicateImage_ReturnsExisting));
            var smallUri = new Uri("https://s.png");
            var largeUri = new Uri("https://l.png");
            var existing = new CardImage { CardImageId = 1, Small = smallUri, Large = largeUri };
            db.CardImages.Add(existing);
            await db.SaveChangesAsync();

            var repo = new CardImageRepository(db, _logger.Object);
            var duplicate = new CardImage { Small = smallUri, Large = largeUri };

            var result = await repo.SaveImageCardAsync(duplicate, CancellationToken.None);

            Assert.Equal(1, result.CardImageId);
        }

        // ?? GetAllImagesAsync ????????????????????????????????????????????

        [Fact]
        public async Task GetAllImagesAsync_WithData_ReturnsAll()
        {
            var db = DbContextFactory.Create(nameof(GetAllImagesAsync_WithData_ReturnsAll));
            db.CardImages.AddRange(
                new CardImage { Small = new Uri("https://a.png"), Large = new Uri("https://A.png") },
                new CardImage { Small = new Uri("https://b.png"), Large = new Uri("https://B.png") }
            );
            await db.SaveChangesAsync();

            var repo = new CardImageRepository(db, _logger.Object);
            var result = await repo.GetAllImagesAsync();

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetAllImagesAsync_Empty_ReturnsEmpty()
        {
            var repo = CreateRepo(nameof(GetAllImagesAsync_Empty_ReturnsEmpty));

            var result = await repo.GetAllImagesAsync();

            Assert.Empty(result);
        }
    }
}
