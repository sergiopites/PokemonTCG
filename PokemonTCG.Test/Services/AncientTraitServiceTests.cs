using Microsoft.Extensions.Logging;
using Moq;
using PokemonTCG.API.Models;
using PokemonTCG.API.Repositories;
using PokemonTCG.API.Services;

namespace PokemonTCG.Test.Services
{
    public class AncientTraitServiceTests
    {
        private readonly Mock<IAncientTraitRepository> _repo = new();
        private readonly Mock<ILogger<AncientTraitService>> _logger = new();

        private AncientTraitService CreateService() => new(_repo.Object, _logger.Object);

        [Fact]
        public void SaveAntientTraitAsync_WithTrait_CallsRepository()
        {
            var trait = new AncientTrait { Name = "? Stop", Text = "Prevents effects" };

            CreateService().SaveAntientTraitAsync(trait);

            _repo.Verify(r => r.SaveAncientTraitAsync(trait, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void SaveAntientTraitAsync_NullTrait_DoesNotCallRepository()
        {
            CreateService().SaveAntientTraitAsync(null!);

            _repo.Verify(r => r.SaveAncientTraitAsync(It.IsAny<AncientTrait>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
