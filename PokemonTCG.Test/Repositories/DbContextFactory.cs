using Microsoft.EntityFrameworkCore;
using PokemonTCG.API.Data;

namespace PokemonTCG.Test.Repositories
{
    internal static class DbContextFactory
    {
        public static AppDbContext Create(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            var context = new AppDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }
    }
}
