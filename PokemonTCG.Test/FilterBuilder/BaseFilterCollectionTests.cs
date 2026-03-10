using PokemonTCG.SDK.Features.FilterBuilder.Pokemon;
using PokemonTCG.SDK.Features.FilterBuilder.Ordering;

namespace PokemonTCG.Test.FilterBuilder
{
    public class BaseFilterCollectionTests
    {
        private PokemonFilterCollection<string, string> NewFilter() =>
            PokemonFilterBuilder.CreatePokemonFilter();

        [Fact]
        public void Add_KeyValue_IncreasesCount()
        {
            var filter = NewFilter();
            filter.Add("key", "value");
            Assert.Equal(1, filter.Count);
        }

        [Fact]
        public void Add_KeyValuePair_IncreasesCount()
        {
            var filter = NewFilter();
            filter.Add(new KeyValuePair<string, string>("key", "value"));
            Assert.Equal(1, filter.Count);
        }

        [Fact]
        public void Indexer_Get_ReturnsCorrectValue()
        {
            var filter = NewFilter();
            filter.Add("key", "value");
            Assert.Equal("value", filter["key"]);
        }

        [Fact]
        public void Indexer_Set_UpdatesValue()
        {
            var filter = NewFilter();
            filter.Add("key", "old");
            filter["key"] = "new";
            Assert.Equal("new", filter["key"]);
        }

        [Fact]
        public void Clear_RemovesAllEntries()
        {
            var filter = NewFilter();
            filter.Add("key1", "value1");
            filter.Add("key2", "value2");
            filter.Clear();
            Assert.Equal(0, filter.Count);
        }

        [Fact]
        public void ContainsKey_ExistingKey_ReturnsTrue()
        {
            var filter = NewFilter();
            filter.Add("key", "value");
            Assert.True(filter.ContainsKey("key"));
        }

        [Fact]
        public void ContainsKey_MissingKey_ReturnsFalse()
        {
            var filter = NewFilter();
            Assert.False(filter.ContainsKey("missing"));
        }

        [Fact]
        public void Contains_ExistingPair_ReturnsTrue()
        {
            var filter = NewFilter();
            filter.Add("key", "value");
            Assert.True(filter.Contains(new KeyValuePair<string, string>("key", "value")));
        }

        [Fact]
        public void Remove_ByKey_RemovesEntry()
        {
            var filter = NewFilter();
            filter.Add("key", "value");
            filter.Remove("key");
            Assert.False(filter.ContainsKey("key"));
        }

        [Fact]
        public void Remove_ByKeyValuePair_RemovesEntry()
        {
            var filter = NewFilter();
            filter.Add("key", "value");
            filter.Remove(new KeyValuePair<string, string>("key", "value"));
            Assert.False(filter.ContainsKey("key"));
        }

        [Fact]
        public void TryGetValue_ExistingKey_ReturnsTrueAndValue()
        {
            var filter = NewFilter();
            filter.Add("key", "value");
            var found = filter.TryGetValue("key", out var val);
            Assert.True(found);
            Assert.Equal("value", val);
        }

        [Fact]
        public void TryGetValue_MissingKey_ReturnsFalse()
        {
            var filter = NewFilter();
            var found = filter.TryGetValue("missing", out _);
            Assert.False(found);
        }

        [Fact]
        public void Keys_ReturnsAllKeys()
        {
            var filter = NewFilter();
            filter.Add("k1", "v1");
            filter.Add("k2", "v2");
            Assert.Contains("k1", filter.Keys);
            Assert.Contains("k2", filter.Keys);
        }

        [Fact]
        public void Values_ReturnsAllValues()
        {
            var filter = NewFilter();
            filter.Add("k1", "v1");
            filter.Add("k2", "v2");
            Assert.Contains("v1", filter.Values);
            Assert.Contains("v2", filter.Values);
        }

        [Fact]
        public void IsReadOnly_ReturnsFalse()
        {
            var filter = NewFilter();
            Assert.False(filter.IsReadOnly);
        }

        [Fact]
        public void CopyTo_CopiesEntriesToArray()
        {
            var filter = NewFilter();
            filter.Add("key", "value");
            var array = new KeyValuePair<string, string>[1];
            filter.CopyTo(array, 0);
            Assert.Equal("key", array[0].Key);
            Assert.Equal("value", array[0].Value);
        }

        [Fact]
        public void GetEnumerator_IteratesAllEntries()
        {
            var filter = NewFilter();
            filter.Add("k1", "v1");
            filter.Add("k2", "v2");
            var count = 0;
            foreach (var _ in filter) count++;
            Assert.Equal(2, count);
        }

        [Fact]
        public void GetEnumerator_NonGeneric_IteratesAllEntries()
        {
            var filter = NewFilter();
            filter.Add("k1", "v1");
            var count = 0;
            var enumerator = ((System.Collections.IEnumerable)filter).GetEnumerator();
            while (enumerator.MoveNext()) count++;
            Assert.Equal(1, count);
        }

        [Fact]
        public void OrderBy_Ascending_AddsOrderByKey()
        {
            var filter = NewFilter();
            filter.Add("name", "Pikachu");
            var ordered = filter.OrderBy("name");
            Assert.NotNull(ordered);
            Assert.True(filter.ContainsKey("orderby"));
        }

        [Fact]
        public void OrderBy_Descending_AddsNegatedOrderByKey()
        {
            var filter = NewFilter();
            filter.Add("name", "Pikachu");
            filter.OrderBy("name", Ordering.Descending);
            Assert.True(filter.ContainsKey("orderby"));
            Assert.Equal("-name", filter["orderby"]);
        }
    }
}
