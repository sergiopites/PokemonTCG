using Microsoft.Extensions.Logging;
using Moq;
using PokemonTCG.API.Models;
using PokemonTCG.API.Repositories;

namespace PokemonTCG.Test.Repositories
{
    public class SetRepositoryTests
    {
        private readonly Mock<ILogger<SetRepository>> _logger = new();

        private (SetRepository repo, API.Data.AppDbContext db) CreateRepo(string dbName)
        {
            var db = DbContextFactory.Create(dbName);
            return (new SetRepository(db, _logger.Object), db);
        }

        private static Set MakeSet(string id, string name = "Base Set", string series = "Base") =>
            new() { SetId = id, Name = name, Series = series };

        // ?? GetAllSetsAsync ??????????????????????????????????????????????

        [Fact]
        public async Task GetAllSetsAsync_WithData_ReturnsAll()
        {
            var (repo, db) = CreateRepo(nameof(GetAllSetsAsync_WithData_ReturnsAll));
            db.Sets.AddRange(MakeSet("base1", "Base"), MakeSet("base2", "Jungle"));
            await db.SaveChangesAsync();

            var result = await repo.GetAllSetsAsync();

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetAllSetsAsync_Empty_ReturnsEmpty()
        {
            var (repo, _) = CreateRepo(nameof(GetAllSetsAsync_Empty_ReturnsEmpty));

            var result = await repo.GetAllSetsAsync();

            Assert.Empty(result);
        }

        // ?? GetSetByIdAsync ??????????????????????????????????????????????

        [Fact]
        public async Task GetSetByIdAsync_ExistingId_ReturnsSet()
        {
            var (repo, db) = CreateRepo(nameof(GetSetByIdAsync_ExistingId_ReturnsSet));
            db.Sets.Add(MakeSet("base1", "Base Set"));
            await db.SaveChangesAsync();

            var result = await repo.GetSetByIdAsync("base1");

            Assert.Single(result);
            Assert.Equal("Base Set", result[0].Name);
        }

        [Fact]
        public async Task GetSetByIdAsync_NonExistingId_ReturnsEmpty()
        {
            var (repo, _) = CreateRepo(nameof(GetSetByIdAsync_NonExistingId_ReturnsEmpty));

            var result = await repo.GetSetByIdAsync("missing");

            Assert.Empty(result);
        }

        // ?? GetSetByNameAsync ????????????????????????????????????????????

        [Fact]
        public async Task GetSetByNameAsync_ExistingName_ReturnsSet()
        {
            var (repo, db) = CreateRepo(nameof(GetSetByNameAsync_ExistingName_ReturnsSet));
            db.Sets.Add(MakeSet("base1", "Base Set"));
            await db.SaveChangesAsync();

            var result = await repo.GetSetByNameAsync("Base Set");

            Assert.Single(result);
        }

        [Fact]
        public async Task GetSetByNameAsync_NonExistingName_ReturnsEmpty()
        {
            var (repo, _) = CreateRepo(nameof(GetSetByNameAsync_NonExistingName_ReturnsEmpty));

            var result = await repo.GetSetByNameAsync("Unknown");

            Assert.Empty(result);
        }

        // ?? GetSetBySerieAsync ???????????????????????????????????????????

        [Fact]
        public async Task GetSetBySerieAsync_ExistingSerie_ReturnsSets()
        {
            var (repo, db) = CreateRepo(nameof(GetSetBySerieAsync_ExistingSerie_ReturnsSets));
            db.Sets.AddRange(MakeSet("base1", "Base Set", "Base"), MakeSet("jungle1", "Jungle", "Base"));
            await db.SaveChangesAsync();

            var result = await repo.GetSetBySerieAsync("Base");

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetSetBySerieAsync_NonExistingSerie_ReturnsEmpty()
        {
            var (repo, _) = CreateRepo(nameof(GetSetBySerieAsync_NonExistingSerie_ReturnsEmpty));

            var result = await repo.GetSetBySerieAsync("Nonexistent");

            Assert.Empty(result);
        }

        // ?? SaveSetAsync ?????????????????????????????????????????????????

        [Fact]
        public async Task SaveSetAsync_NewSet_SavesAndReturns()
        {
            var (repo, _) = CreateRepo(nameof(SaveSetAsync_NewSet_SavesAndReturns));
            var set = MakeSet("neo1", "Neo Genesis");

            var result = await repo.SaveSetAsync(set);

            Assert.Equal("neo1", result.SetId);
        }

        [Fact]
        public async Task SaveSetAsync_DuplicateSet_ReturnsExisting()
        {
            var (repo, db) = CreateRepo(nameof(SaveSetAsync_DuplicateSet_ReturnsExisting));
            db.Sets.Add(MakeSet("base1", "Base Set"));
            await db.SaveChangesAsync();

            var result = await repo.SaveSetAsync(MakeSet("base1", "Different Name"));

            Assert.Equal("Base Set", result.Name);
        }
    }
}
