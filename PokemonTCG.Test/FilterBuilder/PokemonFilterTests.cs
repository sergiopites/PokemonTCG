using PokemonTCG.SDK.Features.FilterBuilder.Pokemon;

namespace PokemonTCG.Test.FilterBuilder
{
    public class PokemonFilterTests
    {
        private PokemonFilterCollection<string, string> NewFilter() =>
            PokemonFilterBuilder.CreatePokemonFilter();

        // AddId
        [Fact]
        public void AddId_AddsIdFilter()
        {
            var filter = NewFilter().AddId("dp4-3");
            Assert.True(filter.ContainsKey("id"));
            Assert.Equal("dp4-3", filter["id"]);
        }

        [Fact]
        public void AddId_Twice_CreatesOrFilter()
        {
            var filter = NewFilter().AddId("dp4-3").AddId("dp4-4");
            Assert.Equal("dp4-3,dp4-4", filter["id"]);
        }

        // AddName
        [Fact]
        public void AddName_AddsNameFilter()
        {
            var filter = NewFilter().AddName("Pikachu");
            Assert.True(filter.ContainsKey("name"));
            Assert.Equal("Pikachu", filter["name"]);
        }

        [Fact]
        public void AddName_Twice_CreatesOrFilter()
        {
            var filter = NewFilter().AddName("Pikachu").AddName("Charizard");
            Assert.Equal("Pikachu,Charizard", filter["name"]);
        }

        // AddSubTypes
        [Fact]
        public void AddSubTypes_AddsSubtypesFilter()
        {
            var filter = NewFilter().AddSubTypes("Basic");
            Assert.True(filter.ContainsKey("subtypes"));
            Assert.Equal("Basic", filter["subtypes"]);
        }

        // AddHpRange
        [Fact]
        public void AddHpRange_Inclusive_UsesSquareBrackets()
        {
            var filter = NewFilter().AddHpRange("60", "120", true);
            Assert.Equal("[60 TO 120]", filter["hp"]);
        }

        [Fact]
        public void AddHpRange_Exclusive_UsesCurlyBrackets()
        {
            var filter = NewFilter().AddHpRange("60", "120", false);
            Assert.Equal("{60 TO 120}", filter["hp"]);
        }

        // AddTypes
        [Fact]
        public void AddTypes_AddsTypesFilter()
        {
            var filter = NewFilter().AddTypes("Fire");
            Assert.True(filter.ContainsKey("types"));
            Assert.Equal("Fire", filter["types"]);
        }

        // AddEvolvesFrom
        [Fact]
        public void AddEvolvesFrom_AddsEvolvesFromFilter()
        {
            var filter = NewFilter().AddEvolvesFrom("Pikachu");
            Assert.True(filter.ContainsKey("evolvesfrom"));
            Assert.Equal("Pikachu", filter["evolvesfrom"]);
        }

        // AddEvolvesTo
        [Fact]
        public void AddEvolvesTo_AddsEvolvesToFilter()
        {
            var filter = NewFilter().AddEvolvesTo("Raichu");
            Assert.True(filter.ContainsKey("evolvesto"));
            Assert.Equal("Raichu", filter["evolvesto"]);
        }

        // AddAttackCostRange
        [Fact]
        public void AddAttackCostRange_Inclusive_UsesSquareBrackets()
        {
            var filter = NewFilter().AddAttackCostRange("1", "3", true);
            Assert.Equal("[1 TO 3]", filter["attacks.convertedEnergyCost"]);
        }

        [Fact]
        public void AddAttackCostRange_Exclusive_UsesCurlyBrackets()
        {
            var filter = NewFilter().AddAttackCostRange("1", "3", false);
            Assert.Equal("{1 TO 3}", filter["attacks.convertedEnergyCost"]);
        }

        // AddSetName
        [Fact]
        public void AddSetName_AddsSetNameFilter()
        {
            var filter = NewFilter().AddSetName("Base");
            Assert.True(filter.ContainsKey("set.name"));
            Assert.Equal("Base", filter["set.name"]);
        }

        // AddSetSeries
        [Fact]
        public void AddSetSeries_AddsSetSeriesFilter()
        {
            var filter = NewFilter().AddSetSeries("Base");
            Assert.True(filter.ContainsKey("set.series"));
            Assert.Equal("Base", filter["set.series"]);
        }

        // AddSetId
        [Fact]
        public void AddSetId_AddsSetIdFilter()
        {
            var filter = NewFilter().AddSetId("base1");
            Assert.True(filter.ContainsKey("set.id"));
            Assert.Equal("base1", filter["set.id"]);
        }

        // AddRarity
        [Fact]
        public void AddRarity_AddsRarityFilter()
        {
            var filter = NewFilter().AddRarity("Rare");
            Assert.True(filter.ContainsKey("rarity"));
            Assert.Equal("Rare", filter["rarity"]);
        }

        // HasAncientTrait
        [Fact]
        public void HasAncientTrait_AddsAncientTraitFilter()
        {
            var filter = NewFilter().HasAncientTrait();
            Assert.True(filter.ContainsKey("ancientTrait.name"));
        }

        [Fact]
        public void HasAncientTrait_CalledTwice_DoesNotDuplicate()
        {
            var filter = NewFilter().HasAncientTrait().HasAncientTrait();
            Assert.Single(filter.Where(x => x.Key == "ancientTrait.name"));
        }

        // Chaining
        [Fact]
        public void MultipleFilters_CanBeChained()
        {
            var filter = NewFilter()
                .AddName("Pikachu")
                .AddTypes("Lightning")
                .AddSetId("base1");

            Assert.Equal(3, filter.Count);
        }
    }
}
