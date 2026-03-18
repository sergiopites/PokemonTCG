using Microsoft.Extensions.Logging;
using Moq;
using PokemonTCG.API.Repositories;

namespace PokemonTCG.Test.Repositories
{
    public class TCGPlayerRepositoryTests
    {
        [Fact]
        public void Constructor_ValidDependencies_CreatesInstance()
        {
            var db = DbContextFactory.Create(nameof(Constructor_ValidDependencies_CreatesInstance));
            var logger = new Mock<ILogger<TCGPlayerRepository>>();

            var repo = new TCGPlayerRepository(db, logger.Object);

            Assert.NotNull(repo);
        }
    }
}
