using PokemonTCG.SDK.Features.FilterBuilder.Pokemon;

namespace PokemonTCG.Test.FilterBuilder
{
    public class PokemonFilterBuilderTests
    {
        [Fact]
        public void CreatePokemonFilter_ReturnsNonNullCollection()
        {
            var filter = PokemonFilterBuilder.CreatePokemonFilter();
            Assert.NotNull(filter);
        }

        [Fact]
        public void CreatePokemonFilter_ReturnsEmptyCollection()
        {
            var filter = PokemonFilterBuilder.CreatePokemonFilter();
            Assert.Empty(filter);
        }

        [Fact]
        public void CreatePokemonFilter_ReturnsDifferentInstancesEachCall()
        {
            var filter1 = PokemonFilterBuilder.CreatePokemonFilter();
            var filter2 = PokemonFilterBuilder.CreatePokemonFilter();
            Assert.NotSame(filter1, filter2);
        }
    }
}
