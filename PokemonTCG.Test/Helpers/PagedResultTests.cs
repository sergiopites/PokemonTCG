using PokemonTCG.API.Helpers;

namespace PokemonTCG.Test.Helpers
{
    public class PagedResultTests
    {
        [Fact]
        public void PagedResult_DefaultValues_AreZeroAndNull()
        {
            var result = new PagedResult<string>();

            Assert.Equal(0, result.Page);
            Assert.Equal(0, result.PageSize);
            Assert.Equal(0, result.TotalCount);
            Assert.Null(result.Items);
        }

        [Fact]
        public void PagedResult_SetProperties_ReturnCorrectValues()
        {
            var items = new List<string> { "a", "b", "c" };

            var result = new PagedResult<string>
            {
                Page = 2,
                PageSize = 10,
                TotalCount = 100,
                Items = items
            };

            Assert.Equal(2, result.Page);
            Assert.Equal(10, result.PageSize);
            Assert.Equal(100, result.TotalCount);
            Assert.Equal(items, result.Items);
        }

        [Fact]
        public void PagedResult_WithIntItems_WorksCorrectly()
        {
            var result = new PagedResult<int>
            {
                Page = 1,
                PageSize = 5,
                TotalCount = 3,
                Items = new List<int> { 1, 2, 3 }
            };

            Assert.Equal(3, result.Items.Count());
        }

        [Fact]
        public void PagedResult_WithEmptyItems_ReturnsEmptyEnumerable()
        {
            var result = new PagedResult<string>
            {
                Items = Enumerable.Empty<string>()
            };

            Assert.Empty(result.Items);
        }
    }
}
