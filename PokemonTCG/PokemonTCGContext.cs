using PokemonTCG.Models;
using System.Data.Entity;
using Microsoft.EntityFrameworkCore;

public class PokemonTCGContext : DbContext
{
    public DbSet<CardSet> CardSets { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Server=SERGIOPITES;Database=PokemonTCG;Trusted_Connection=True;");        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CardSet>().OwnsOne(c => c.CardCount);
    }
}
