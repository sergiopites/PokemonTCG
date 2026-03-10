using PokemonTCG.API.Helpers;

namespace PokemonTCG.Test.Helpers
{
    public class CacheKeyHelperTests
    {
        [Fact]
        public void Hash_SingleValue_ReturnsSha256HexString()
        {
            var result = CacheKeyHelper.Hash("test");

            Assert.NotNull(result);
            Assert.Equal(64, result.Length);
        }

        [Fact]
        public void Hash_SameInputs_ReturnsSameHash()
        {
            var result1 = CacheKeyHelper.Hash("a", "b", "c");
            var result2 = CacheKeyHelper.Hash("a", "b", "c");

            Assert.Equal(result1, result2);
        }

        [Fact]
        public void Hash_DifferentInputs_ReturnsDifferentHashes()
        {
            var result1 = CacheKeyHelper.Hash("a", "b");
            var result2 = CacheKeyHelper.Hash("x", "y");

            Assert.NotEqual(result1, result2);
        }

        [Fact]
        public void Hash_NullValue_UsesPlaceholder()
        {
            var result = CacheKeyHelper.Hash(null, "value");

            Assert.NotNull(result);
            Assert.Equal(64, result.Length);
        }

        [Fact]
        public void Hash_IsCaseInsensitive()
        {
            var lower = CacheKeyHelper.Hash("pokemon");
            var upper = CacheKeyHelper.Hash("POKEMON");

            Assert.Equal(lower, upper);
        }

        [Fact]
        public void Hash_TrimsWhitespace()
        {
            var trimmed = CacheKeyHelper.Hash("value");
            var withSpaces = CacheKeyHelper.Hash("  value  ");

            Assert.Equal(trimmed, withSpaces);
        }

        [Fact]
        public void Hash_MultipleValues_CombinesWithPipe()
        {
            var combined = CacheKeyHelper.Hash("a", "b");
            var single = CacheKeyHelper.Hash("a");

            Assert.NotEqual(combined, single);
        }

        [Fact]
        public void Hash_EmptyParams_ReturnsValidHash()
        {
            var result = CacheKeyHelper.Hash();

            Assert.NotNull(result);
            Assert.Equal(64, result.Length);
        }
    }
}
