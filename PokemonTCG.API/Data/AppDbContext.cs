using Microsoft.EntityFrameworkCore;
using PokemonTCG.API.Models;
using System.Linq;

namespace PokemonTCG.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Ability> Abilities { get; set; }
        public DbSet<AncientTrait> AncientTraits { get; set; }
        public DbSet<Attack> Attacks { get; set; }
        public DbSet<Card> Cards { get; set; }
        public DbSet<CardImage> CardImages { get; set; }
        public DbSet<CardMarket> CardMarkets { get; set; }
        public DbSet<CardMarketPrice> CardMarketPrices { get; set; }
        public DbSet<SetImage> SetImages { get; set; }
        public DbSet<Legality> Legalities { get; set; }
        public DbSet<Price> Prices { get; set; }
        public DbSet<Resistance> Resistances { get; set; }
        public DbSet<Weakness> Weaknesses { get; set; }
        public DbSet<Set> Sets { get; set; }
        public DbSet<TCGPlayer> TCGPlayers { get; set; }
        public DbSet<TCGPlayerPrice> TCGPlayerPrices { get; set; }
        public DbSet<Deck> Decks { get; set; }
        public DbSet<DeckCard> DeckCards { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ---------------- CARD ----------------
            modelBuilder.Entity<Card>(entity =>
            {
                entity.HasKey(c => c.CardId);
                entity.HasIndex(c => c.ExternalId).IsUnique();
                entity.Property(c => c.ExternalId).IsRequired().HasMaxLength(100);

                // 1:1 Opcional
                entity.HasOne(c => c.CardImage)
                      .WithOne(ci => ci.Card)
                      .HasForeignKey<Card>(c => c.CardImageId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.Legalities)
                      .WithOne()
                      .HasForeignKey<Card>(c => c.LegalitiesId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.Restrict);

                // 1:N
                entity.HasMany(c => c.Abilities).WithOne(a => a.Card)
                      .HasForeignKey(a => a.CardId).OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(c => c.Attacks).WithOne(a => a.Card)
                      .HasForeignKey(a => a.CardId).OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(c => c.Resistances).WithOne(r => r.Card)
                      .HasForeignKey(r => r.CardId).OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(c => c.Weaknesses).WithOne(w => w.Card)
                      .HasForeignKey(w => w.CardId).OnDelete(DeleteBehavior.Cascade);

                // 1:1 obligatorio
                entity.HasOne(c => c.CardMarket)
                      .WithOne(cm => cm.Card)
                      .HasForeignKey<CardMarket>(cm => cm.CardId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.Tcgplayer)
                      .WithOne(tp => tp.Card)
                      .HasForeignKey<TCGPlayer>(tp => tp.CardId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ---------------- ABILITY ----------------
            modelBuilder.Entity<Ability>().HasKey(a => a.AbilityId);

            // ---------------- RESISTANCE ----------------
            modelBuilder.Entity<Resistance>().HasKey(r => r.ResistanceId);

            // ---------------- WEAKNESS ----------------
            modelBuilder.Entity<Weakness>().HasKey(w => w.WeaknessId);

            // ---------------- ATTACK ----------------
            modelBuilder.Entity<Attack>().HasKey(a => a.AttackId);

            // ---------------- CARD IMAGE ----------------
            modelBuilder.Entity<CardImage>().HasKey(ci => ci.CardImageId);

            // ---------------- ANCIENT TRAIT ----------------
            modelBuilder.Entity<AncientTrait>().HasKey(at => at.AncientTraitId);

            // ---------------- LEGALITY ----------------
            modelBuilder.Entity<Legality>().HasKey(l => l.LegalityId);

            // ---------------- CARD MARKET ----------------
            modelBuilder.Entity<CardMarket>(entity =>
            {
                entity.HasKey(cm => cm.CardMarketId);

                entity.HasMany(cm => cm.CardMarketPrices)
                      .WithOne(cmp => cmp.CardMarket)
                      .HasForeignKey(cmp => cmp.CardMarketId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ---------------- CARD MARKET PRICE ----------------
            modelBuilder.Entity<CardMarketPrice>(entity =>
            {
                entity.HasKey(cmp => cmp.CardMarketId);
                entity.Property(p => p.AverageSellPrice).HasColumnType("decimal(18,2)");
                entity.Property(p => p.LowPrice).HasColumnType("decimal(18,2)");
                entity.Property(p => p.TrendPrice).HasColumnType("decimal(18,2)");
                entity.Property(p => p.ReverseHoloLow).HasColumnType("decimal(18,2)");
                entity.Property(p => p.ReverseHoloTrend).HasColumnType("decimal(18,2)");
                entity.Property(p => p.LowPriceExPlus).HasColumnType("decimal(18,2)");
                entity.Property(p => p.AverageDay).HasColumnType("decimal(18,2)");
                entity.Property(p => p.AverageWeek).HasColumnType("decimal(18,2)");
                entity.Property(p => p.AverageMonth).HasColumnType("decimal(18,2)");
                entity.Property(p => p.AverageDayReverseHolo).HasColumnType("decimal(18,2)");
                entity.Property(p => p.AverageWeekReverseHolo).HasColumnType("decimal(18,2)");
                entity.Property(p => p.AverageMonthReverseHolo).HasColumnType("decimal(18,2)");
            });


            // ---------------- TCGPLAYER ----------------
            modelBuilder.Entity<TCGPlayer>(entity =>
            {
                entity.HasMany(tp => tp.TCGPlayerPrices)
                      .WithOne(tpp => tpp.TCGPlayer)
                      .HasForeignKey(tpp => tpp.TCGPlayerId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ---------------- TCGPLAYER PRICE ----------------
            modelBuilder.Entity<TCGPlayerPrice>(entity =>
            {
                entity.HasKey(tpp => tpp.TCGPlayerPriceId);

                entity.HasMany(tpp => tpp.Prices)
                      .WithOne(p => p.TCGPlayerPrice)
                      .HasForeignKey(p => p.TCGPlayerPriceId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ---------------- PRICE ----------------
            modelBuilder.Entity<Price>(entity =>
            {
                entity.HasKey(p => p.PriceId);
                entity.HasOne(p => p.TCGPlayerPrice)
                      .WithMany(tpp => tpp.Prices)
                      .HasForeignKey(tpp => tpp.TCGPlayerPriceId);

                entity.Property(p => p.Type)
                      .HasConversion<int>()
                      .IsRequired();

                entity.HasIndex(p => new { p.TCGPlayerPriceId, p.Type })
                      .IsUnique();
            });

            // ---------------- DECK ----------------

            modelBuilder.Entity<Deck>()
                .HasMany(d => d.DeckCards)
                .WithOne(dc => dc.Deck)
                .HasForeignKey(dc => dc.DeckId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<DeckCard>(entity =>
            {
                entity.HasKey(dc => dc.DeckCardId);

                entity.HasOne(dc => dc.Card)
                      .WithMany(c => c.DeckCards)
                      .HasForeignKey(dc => dc.CardId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

        }
    }
}