using Microsoft.Extensions.Logging;
using Moq;
using PokemonTCG.API.Models;
using PokemonTCG.API.Repositories;

namespace PokemonTCG.Test.Repositories
{
    public class SetImageRepositoryTests
    {
        private readonly Mock<ILogger<SetImageRepository>> _logger = new();

        private SetImageRepository CreateRepo(string dbName) =>
            new(DbContextFactory.Create(dbName), _logger.Object);

        // ?? SaveImageCardAsync ???????????????????????????????????????????

        [Fact]
        public async Task SaveImageCardAsync_NewSetImage_SavesAndReturns()
        {
            var repo = CreateRepo(nameof(SaveImageCardAsync_NewSetImage_SavesAndReturns));
            var logoUri = new Uri("https://logo.png");
            var symbolUri = new Uri("https://symbol.png");
            var image = new SetImage { Logo = logoUri, Symbol = symbolUri };

            var result = await repo.SaveImageCardAsync(image, CancellationToken.None);

            Assert.Equal(logoUri, result.Logo);
            Assert.Equal(symbolUri, result.Symbol);
        }

        [Fact]
        public async Task SaveImageCardAsync_DuplicateSetImage_ReturnsExisting()
        {
            var db = DbContextFactory.Create(nameof(SaveImageCardAsync_DuplicateSetImage_ReturnsExisting));
            var logoUri = new Uri("https://logo.png");
            var symbolUri = new Uri("https://sym.png");
            var existing = new SetImage { SetImageId = 1, Logo = logoUri, Symbol = symbolUri };
            db.SetImages.Add(existing);
            await db.SaveChangesAsync();

            var repo = new SetImageRepository(db, _logger.Object);
            var duplicate = new SetImage { Logo = logoUri, Symbol = symbolUri };

            var result = await repo.SaveImageCardAsync(duplicate, CancellationToken.None);

            Assert.Equal(1, result.SetImageId);
        }

        [Fact]
        public async Task SaveImageCardAsync_NullLogoAndSymbol_SavesWithNulls()
        {
            var repo = CreateRepo(nameof(SaveImageCardAsync_NullLogoAndSymbol_SavesWithNulls));
            var image = new SetImage { Logo = null, Symbol = null };

            var result = await repo.SaveImageCardAsync(image, CancellationToken.None);

            Assert.Null(result.Logo);
            Assert.Null(result.Symbol);
        }
    }
}
