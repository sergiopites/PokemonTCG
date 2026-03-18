using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using PokemonTCG.API.Data;
using PokemonTCG.API.Models;
using PokemonTCG.API.Repositories;

namespace PokemonTCG.Test.Repositories
{
    public class CardRepositoryTests
    {
        private readonly Mock<ILogger<ICardRepository>> _logger = new();

        private static AppDbContext CreateDb(string dbName) => DbContextFactory.Create(dbName);

        private CardRepository CreateRepo(string dbName) =>
            new(CreateDb(dbName), _logger.Object, new MemoryCache(new MemoryCacheOptions()));

        private CardRepository CreateRepo(AppDbContext db) =>
            new(db, _logger.Object, new MemoryCache(new MemoryCacheOptions()));

        // Seed helpers
        private static async Task SeedBaseSet(AppDbContext db)
        {
            if (!await db.Sets.AnyAsync(s => s.SetId == "base1"))
            {
                db.Sets.Add(new Set { SetId = "base1", Name = "Base Set", PrintedTotal = 102, Total = 102, PtcgoCode = "BS", Series = "Base", ReleaseDate = "1999/01/09" });
                await db.SaveChangesAsync();
            }
        }

        private static async Task<Card> SeedFullCard(AppDbContext db, string cardId, string externalId, string? name = "Pikachu",
            string? types = "Lightning", string? rarity = "Common", string? superType = "Pokémon", string? subTypes = "Basic",
            string? number = "25")
        {
            await SeedBaseSet(db);

            var cardImage = new CardImage { Small = new Uri("https://img.test/s.png"), Large = new Uri("https://img.test/l.png") };
            db.CardImages.Add(cardImage);
            await db.SaveChangesAsync();

            var card = new Card
            {
                CardId = cardId,
                ExternalId = externalId,
                Name = name,
                Types = types,
                Rarity = rarity,
                SuperType = superType,
                SubTypes = subTypes,
                Number = number,
                SetId = "base1",
                CardImageId = cardImage.CardImageId,
                Hp = 60,
                Artist = "Ken Sugimori",
                Abilities = new List<Ability>
                {
                    new() { Name = "Static", Text = "May paralyze", Type = "Ability" }
                },
                Attacks = new List<Attack>
                {
                    new() { Name = "Thunder Shock", Text = "Flip a coin", Damage = "10", CostJson = "[\"Lightning\"]", ConvertedEnergyCost = "1" }
                },
                Resistances = new List<Resistance>
                {
                    new() { Type = "Fighting", Value = "-20" }
                },
                Weaknesses = new List<Weakness>
                {
                    new() { Type = "Ground", Value = "×2" }
                },
                CardMarket = new CardMarket
                {
                    Url = new Uri("https://cardmarket.test"),
                    UpdatedAt = "2024-01-01",
                    CardMarketPrices = new List<CardMarketPrice>
                    {
                        new() { AverageSellPrice = 1.5m, LowPrice = 0.5m, TrendPrice = 1.0m }
                    }
                },
                Tcgplayer = new TCGPlayer
                {
                    Url = new Uri("https://tcgplayer.test"),
                    UpdatedAt = "2024-01-01",
                    TCGPlayerPrices = new List<TCGPlayerPrice>
                    {
                        new()
                        {
                            Prices = new List<Price>
                            {
                                new() { Type = PriceType.Normal, Low = 0.5, Mid = 1.0, High = 2.0, Market = 1.0, DirectLow = 0.4 }
                            }
                        }
                    }
                }
            };

            db.Cards.Add(card);
            await db.SaveChangesAsync();
            return card;
        }

        // ?? GetCardsByCardIdAsync ???????????????????????????????????????

        [Fact]
        public async Task GetCardsByCardIdAsync_ExistingCard_ReturnsCardDetail()
        {
            var db = CreateDb(nameof(GetCardsByCardIdAsync_ExistingCard_ReturnsCardDetail));
            await SeedFullCard(db, "pika-1", "pika-ext-1");

            var repo = CreateRepo(db);
            var result = await repo.GetCardsByCardIdAsync("pika-1");

            Assert.Single(result);
            Assert.Equal("pika-1", result[0].CardId);
            Assert.Equal("Pikachu", result[0].Name);
            Assert.NotNull(result[0].ImageLarge);
            Assert.NotEmpty(result[0].AttackDetails);
            Assert.NotEmpty(result[0].AbilityDetails);
            Assert.NotEmpty(result[0].ResistanceDetails);
            Assert.NotEmpty(result[0].WeaknessDetails);
        }

        [Fact]
        public async Task GetCardsByCardIdAsync_NonExistentCard_ReturnsEmpty()
        {
            var repo = CreateRepo(nameof(GetCardsByCardIdAsync_NonExistentCard_ReturnsEmpty));

            var result = await repo.GetCardsByCardIdAsync("no-such-card");

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetCardsByCardIdAsync_DbError_ReturnsEmpty()
        {
            var db = CreateDb(nameof(GetCardsByCardIdAsync_DbError_ReturnsEmpty));
            db.Dispose();

            var repo = new CardRepository(db, _logger.Object, new MemoryCache(new MemoryCacheOptions()));
            var result = await repo.GetCardsByCardIdAsync("any-id");

            Assert.Empty(result);
        }

        // ?? GetCardsBySet ???????????????????????????????????????????????

        [Fact]
        public async Task GetCardsBySet_ExistingSet_ReturnsOrderedCards()
        {
            var db = CreateDb(nameof(GetCardsBySet_ExistingSet_ReturnsOrderedCards));
            await SeedFullCard(db, "card-1", "ext-1", name: "Bulbasaur", number: "1");
            await SeedFullCard(db, "card-10", "ext-10", name: "Caterpie", number: "10");
            await SeedFullCard(db, "card-2", "ext-2", name: "Ivysaur", number: "2");
            await SeedFullCard(db, "card-promo", "ext-promo", name: "Promo", number: "PROMO");

            var repo = CreateRepo(db);
            var result = await repo.GetCardsBySet("base1");

            Assert.Equal(4, result.Count);
            Assert.Equal("1", result[0].Number);
            Assert.Equal("2", result[1].Number);
            Assert.Equal("10", result[2].Number);
            Assert.Equal("PROMO", result[3].Number);
        }

        [Fact]
        public async Task GetCardsBySet_NoCards_ReturnsEmpty()
        {
            var db = CreateDb(nameof(GetCardsBySet_NoCards_ReturnsEmpty));
            await SeedBaseSet(db);

            var repo = CreateRepo(db);
            var result = await repo.GetCardsBySet("nonexistent-set");

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetCardsBySet_DbError_ReturnsEmpty()
        {
            var db = CreateDb(nameof(GetCardsBySet_DbError_ReturnsEmpty));
            db.Dispose();

            var repo = new CardRepository(db, _logger.Object, new MemoryCache(new MemoryCacheOptions()));
            var result = await repo.GetCardsBySet("base1");

            Assert.Empty(result);
        }

        // ?? SearchCardsAsync ????????????????????????????????????????????

        [Fact]
        public async Task SearchCardsAsync_NoFilters_ReturnsAllPaged()
        {
            var db = CreateDb(nameof(SearchCardsAsync_NoFilters_ReturnsAllPaged));
            await SeedFullCard(db, "s1", "se1", name: "Charmander");
            await SeedFullCard(db, "s2", "se2", name: "Squirtle");

            var repo = CreateRepo(db);
            var result = await repo.SearchCardsAsync();

            Assert.Equal(2, result.TotalCount);
            Assert.Equal(2, result.Items.Count());
            Assert.Equal(1, result.Page);
        }

        [Fact]
        public async Task SearchCardsAsync_FilterByName_ReturnsMatching()
        {
            var db = CreateDb(nameof(SearchCardsAsync_FilterByName_ReturnsMatching));
            await SeedFullCard(db, "fn1", "fne1", name: "Charmander");
            await SeedFullCard(db, "fn2", "fne2", name: "Squirtle");

            var repo = CreateRepo(db);
            var result = await repo.SearchCardsAsync(name: "Char");

            Assert.Single(result.Items);
            Assert.Equal("Charmander", result.Items.First().Name);
        }

        [Fact]
        public async Task SearchCardsAsync_FilterBySetId_ReturnsMatching()
        {
            var db = CreateDb(nameof(SearchCardsAsync_FilterBySetId_ReturnsMatching));
            await SeedFullCard(db, "fs1", "fse1");

            var repo = CreateRepo(db);
            var result = await repo.SearchCardsAsync(setId: "base1");

            Assert.Single(result.Items);
        }

        [Fact]
        public async Task SearchCardsAsync_FilterByType_ReturnsMatching()
        {
            var db = CreateDb(nameof(SearchCardsAsync_FilterByType_ReturnsMatching));
            await SeedFullCard(db, "ft1", "fte1", types: "Fire");
            await SeedFullCard(db, "ft2", "fte2", types: "Water");

            var repo = CreateRepo(db);
            var result = await repo.SearchCardsAsync(type: "Fire");

            Assert.Single(result.Items);
        }

        [Fact]
        public async Task SearchCardsAsync_FilterByNumber_ReturnsMatching()
        {
            var db = CreateDb(nameof(SearchCardsAsync_FilterByNumber_ReturnsMatching));
            await SeedFullCard(db, "fnum1", "fnume1", number: "42");
            await SeedFullCard(db, "fnum2", "fnume2", number: "99");

            var repo = CreateRepo(db);
            var result = await repo.SearchCardsAsync(number: "42");

            Assert.Single(result.Items);
        }

        [Fact]
        public async Task SearchCardsAsync_Pagination_RespectsPageSize()
        {
            var db = CreateDb(nameof(SearchCardsAsync_Pagination_RespectsPageSize));
            for (int i = 1; i <= 5; i++)
                await SeedFullCard(db, $"pg{i}", $"pge{i}", name: $"Card{i}", number: i.ToString());

            var repo = CreateRepo(db);
            var page1 = await repo.SearchCardsAsync(page: 1, pageSize: 2);
            var page2 = await repo.SearchCardsAsync(page: 2, pageSize: 2);

            Assert.Equal(5, page1.TotalCount);
            Assert.Equal(2, page1.Items.Count());
            Assert.Equal(2, page2.Items.Count());
        }

        [Fact]
        public async Task SearchCardsAsync_CachedResult_ReturnsSameResult()
        {
            var db = CreateDb(nameof(SearchCardsAsync_CachedResult_ReturnsSameResult));
            await SeedFullCard(db, "cache1", "cachee1");

            var cache = new MemoryCache(new MemoryCacheOptions());
            var repo = new CardRepository(db, _logger.Object, cache);

            var result1 = await repo.SearchCardsAsync(name: "Pikachu");
            var result2 = await repo.SearchCardsAsync(name: "Pikachu");

            Assert.Equal(result1.TotalCount, result2.TotalCount);
            Assert.Equal(result1.Items.Count(), result2.Items.Count());
        }

        [Fact]
        public async Task SearchCardsAsync_NoResults_ReturnsEmptyPaged()
        {
            var repo = CreateRepo(nameof(SearchCardsAsync_NoResults_ReturnsEmptyPaged));

            var result = await repo.SearchCardsAsync(name: "NoSuchCard");

            Assert.Equal(0, result.TotalCount);
            Assert.Empty(result.Items);
        }

        // ?? GetDistinctRaritiesAsync ????????????????????????????????????

        [Fact]
        public async Task GetDistinctRaritiesAsync_ReturnsDistinctSorted()
        {
            var db = CreateDb(nameof(GetDistinctRaritiesAsync_ReturnsDistinctSorted));
            await SeedFullCard(db, "r1", "re1", rarity: "Rare");
            await SeedFullCard(db, "r2", "re2", rarity: "Common");
            await SeedFullCard(db, "r3", "re3", rarity: "Rare");

            var repo = CreateRepo(db);
            var result = (await repo.GetDistinctRaritiesAsync()).ToList();

            Assert.Equal(2, result.Count);
            Assert.Equal("Common", result[0]);
            Assert.Equal("Rare", result[1]);
        }

        [Fact]
        public async Task GetDistinctRaritiesAsync_NoCards_ReturnsEmpty()
        {
            var repo = CreateRepo(nameof(GetDistinctRaritiesAsync_NoCards_ReturnsEmpty));
            var result = await repo.GetDistinctRaritiesAsync();
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetDistinctRaritiesAsync_NullAndEmptyRarity_AreExcluded()
        {
            var db = CreateDb(nameof(GetDistinctRaritiesAsync_NullAndEmptyRarity_AreExcluded));
            await SeedBaseSet(db);
            db.Cards.Add(new Card { CardId = "rn1", ExternalId = "rne1", SetId = "base1", Rarity = null });
            db.Cards.Add(new Card { CardId = "rn2", ExternalId = "rne2", SetId = "base1", Rarity = "" });
            db.Cards.Add(new Card { CardId = "rn3", ExternalId = "rne3", SetId = "base1", Rarity = "Uncommon" });
            await db.SaveChangesAsync();

            var repo = CreateRepo(db);
            var result = (await repo.GetDistinctRaritiesAsync()).ToList();

            Assert.Single(result);
            Assert.Equal("Uncommon", result[0]);
        }

        // ?? GetDistinctTypesAsync ???????????????????????????????????????

        [Fact]
        public async Task GetDistinctTypesAsync_ReturnsDistinctSorted()
        {
            var db = CreateDb(nameof(GetDistinctTypesAsync_ReturnsDistinctSorted));
            await SeedFullCard(db, "t1", "te1", types: "Fire");
            await SeedFullCard(db, "t2", "te2", types: "Water");
            await SeedFullCard(db, "t3", "te3", types: "Fire");

            var repo = CreateRepo(db);
            var result = (await repo.GetDistinctTypesAsync()).ToList();

            Assert.Equal(2, result.Count);
            Assert.Equal("Fire", result[0]);
            Assert.Equal("Water", result[1]);
        }

        [Fact]
        public async Task GetDistinctTypesAsync_NullAndEmptyTypes_AreExcluded()
        {
            var db = CreateDb(nameof(GetDistinctTypesAsync_NullAndEmptyTypes_AreExcluded));
            await SeedBaseSet(db);
            db.Cards.Add(new Card { CardId = "tn1", ExternalId = "tne1", SetId = "base1", Types = null });
            db.Cards.Add(new Card { CardId = "tn2", ExternalId = "tne2", SetId = "base1", Types = "" });
            db.Cards.Add(new Card { CardId = "tn3", ExternalId = "tne3", SetId = "base1", Types = "Grass" });
            await db.SaveChangesAsync();

            var repo = CreateRepo(db);
            var result = (await repo.GetDistinctTypesAsync()).ToList();

            Assert.Single(result);
            Assert.Equal("Grass", result[0]);
        }

        // ?? GetDistinctSupertypesAsync ??????????????????????????????????

        [Fact]
        public async Task GetDistinctSupertypesAsync_ReturnsDistinctSorted()
        {
            var db = CreateDb(nameof(GetDistinctSupertypesAsync_ReturnsDistinctSorted));
            await SeedFullCard(db, "st1", "ste1", superType: "Pokémon");
            await SeedFullCard(db, "st2", "ste2", superType: "Trainer");
            await SeedFullCard(db, "st3", "ste3", superType: "Pokémon");

            var repo = CreateRepo(db);
            var result = (await repo.GetDistinctSupertypesAsync()).ToList();

            Assert.Equal(2, result.Count);
            Assert.Contains("Pokémon", result);
            Assert.Contains("Trainer", result);
        }

        [Fact]
        public async Task GetDistinctSupertypesAsync_NullAndEmpty_AreExcluded()
        {
            var db = CreateDb(nameof(GetDistinctSupertypesAsync_NullAndEmpty_AreExcluded));
            await SeedBaseSet(db);
            db.Cards.Add(new Card { CardId = "stn1", ExternalId = "stne1", SetId = "base1", SuperType = null });
            db.Cards.Add(new Card { CardId = "stn2", ExternalId = "stne2", SetId = "base1", SuperType = "" });
            db.Cards.Add(new Card { CardId = "stn3", ExternalId = "stne3", SetId = "base1", SuperType = "Energy" });
            await db.SaveChangesAsync();

            var repo = CreateRepo(db);
            var result = (await repo.GetDistinctSupertypesAsync()).ToList();

            Assert.Single(result);
            Assert.Equal("Energy", result[0]);
        }

        // ?? GetDistinctSubtypesAsync ????????????????????????????????????

        [Fact]
        public async Task GetDistinctSubtypesAsync_SplitsCommaSeparated_ReturnsDistinctSorted()
        {
            var db = CreateDb(nameof(GetDistinctSubtypesAsync_SplitsCommaSeparated_ReturnsDistinctSorted));
            await SeedFullCard(db, "sub1", "sube1", subTypes: "Basic, Stage 1");
            await SeedFullCard(db, "sub2", "sube2", subTypes: "Stage 1, Stage 2");

            var repo = CreateRepo(db);
            var result = (await repo.GetDistinctSubtypesAsync()).ToList();

            Assert.Equal(3, result.Count);
            Assert.Contains("Basic", result);
            Assert.Contains("Stage 1", result);
            Assert.Contains("Stage 2", result);
        }

        [Fact]
        public async Task GetDistinctSubtypesAsync_NullAndEmpty_AreExcluded()
        {
            var db = CreateDb(nameof(GetDistinctSubtypesAsync_NullAndEmpty_AreExcluded));
            await SeedBaseSet(db);
            db.Cards.Add(new Card { CardId = "subn1", ExternalId = "subne1", SetId = "base1", SubTypes = null });
            db.Cards.Add(new Card { CardId = "subn2", ExternalId = "subne2", SetId = "base1", SubTypes = "" });
            db.Cards.Add(new Card { CardId = "subn3", ExternalId = "subne3", SetId = "base1", SubTypes = "VMAX" });
            await db.SaveChangesAsync();

            var repo = CreateRepo(db);
            var result = (await repo.GetDistinctSubtypesAsync()).ToList();

            Assert.Single(result);
            Assert.Equal("VMAX", result[0]);
        }

        // ?? SaveCardAsync ???????????????????????????????????????????????

        [Fact]
        public async Task SaveCardAsync_NewCard_InsertsAndReturns()
        {
            var db = CreateDb(nameof(SaveCardAsync_NewCard_InsertsAndReturns));
            await SeedBaseSet(db);

            var repo = CreateRepo(db);
            var card = new Card
            {
                CardId = "new-1",
                ExternalId = "new-ext-1",
                Name = "Eevee",
                SetId = "base1",
                Abilities = new List<Ability>(),
                Attacks = new List<Attack>(),
                Resistances = new List<Resistance>(),
                Weaknesses = new List<Weakness>(),
                CardMarket = new CardMarket
                {
                    Url = new Uri("https://cm.test"),
                    UpdatedAt = "2024-01-01",
                    CardMarketPrices = new List<CardMarketPrice>()
                },
                Tcgplayer = new TCGPlayer
                {
                    Url = new Uri("https://tcg.test"),
                    UpdatedAt = "2024-01-01",
                    TCGPlayerPrices = new List<TCGPlayerPrice>()
                }
            };

            var result = await repo.SaveCardAsync(card, CancellationToken.None);

            Assert.Equal("new-1", result.CardId);
            Assert.Equal("Eevee", result.Name);
        }

        [Fact]
        public async Task SaveCardAsync_ExistingCard_UpdatesFields()
        {
            var db = CreateDb(nameof(SaveCardAsync_ExistingCard_UpdatesFields));
            var existing = await SeedFullCard(db, "upd-1", "upd-ext-1", name: "Old Name");

            var repo = CreateRepo(db);
            var updated = new Card
            {
                CardId = "upd-1",
                ExternalId = "upd-ext-1",
                Name = "New Name",
                SetId = "base1",
                Abilities = new List<Ability>
                {
                    new() { Name = "New Ability", Text = "New text", Type = "Ability" }
                },
                Attacks = new List<Attack>
                {
                    new() { Name = "New Attack", Text = "New text", Damage = "50", CostJson = "[\"Fire\"]", ConvertedEnergyCost = "2" }
                },
                Resistances = new List<Resistance>
                {
                    new() { Type = "Water", Value = "-30" }
                },
                Weaknesses = new List<Weakness>
                {
                    new() { Type = "Electric", Value = "×2" }
                },
                CardMarket = new CardMarket
                {
                    Url = new Uri("https://cm-updated.test"),
                    UpdatedAt = "2024-06-01",
                    CardMarketPrices = new List<CardMarketPrice>
                    {
                        new() { AverageSellPrice = 3.0m, LowPrice = 1.0m, TrendPrice = 2.0m }
                    }
                },
                Tcgplayer = new TCGPlayer
                {
                    Url = new Uri("https://tcg-updated.test"),
                    UpdatedAt = "2024-06-01",
                    TCGPlayerPrices = new List<TCGPlayerPrice>
                    {
                        new()
                        {
                            Prices = new List<Price>
                            {
                                new() { Type = PriceType.Holofoil, Low = 1.0, Mid = 2.0, High = 3.0, Market = 2.0, DirectLow = 0.9 }
                            }
                        }
                    }
                }
            };

            var result = await repo.SaveCardAsync(updated, CancellationToken.None);

            Assert.Equal("New Name", result.Name);
        }

        [Fact]
        public async Task SaveCardAsync_ExistingCard_NullCardMarket_AssignsNew()
        {
            var db = CreateDb(nameof(SaveCardAsync_ExistingCard_NullCardMarket_AssignsNew));
            await SeedBaseSet(db);

            // Create a card without CardMarket/TCGPlayer using raw entities
            var card = new Card
            {
                CardId = "ncm-1",
                ExternalId = "ncm-ext-1",
                Name = "NoMarket",
                SetId = "base1",
                Abilities = new List<Ability>(),
                Attacks = new List<Attack>(),
                Resistances = new List<Resistance>(),
                Weaknesses = new List<Weakness>(),
                CardMarket = new CardMarket
                {
                    Url = null,
                    UpdatedAt = null,
                    CardMarketPrices = new List<CardMarketPrice>()
                },
                Tcgplayer = new TCGPlayer
                {
                    Url = null,
                    UpdatedAt = null,
                    TCGPlayerPrices = new List<TCGPlayerPrice>()
                }
            };
            db.Cards.Add(card);
            await db.SaveChangesAsync();

            // Now update with new CardMarket data
            var repo = CreateRepo(db);
            var updateCard = new Card
            {
                CardId = "ncm-1",
                ExternalId = "ncm-ext-1",
                Name = "NoMarket",
                SetId = "base1",
                Abilities = new List<Ability>(),
                Attacks = new List<Attack>(),
                Resistances = new List<Resistance>(),
                Weaknesses = new List<Weakness>(),
                CardMarket = new CardMarket
                {
                    Url = new Uri("https://new-cm.test"),
                    UpdatedAt = "2024-01-01",
                    CardMarketPrices = new List<CardMarketPrice>
                    {
                        new() { AverageSellPrice = 5.0m, LowPrice = 2.0m, TrendPrice = 3.0m }
                    }
                },
                Tcgplayer = new TCGPlayer
                {
                    Url = new Uri("https://new-tcg.test"),
                    UpdatedAt = "2024-01-01",
                    TCGPlayerPrices = new List<TCGPlayerPrice>
                    {
                        new()
                        {
                            Prices = new List<Price>
                            {
                                new() { Type = PriceType.Normal, Low = 1.0, Mid = 2.0, High = 3.0 }
                            }
                        }
                    }
                }
            };

            var result = await repo.SaveCardAsync(updateCard, CancellationToken.None);

            Assert.Equal("ncm-1", result.CardId);
        }

        [Fact]
        public async Task SaveCardAsync_ExistingCard_NullIncomingCollections_UsesEmptyLists()
        {
            var db = CreateDb(nameof(SaveCardAsync_ExistingCard_NullIncomingCollections_UsesEmptyLists));
            await SeedFullCard(db, "nil-1", "nil-ext-1");

            var repo = CreateRepo(db);
            var updated = new Card
            {
                CardId = "nil-1",
                ExternalId = "nil-ext-1",
                Name = "NilCollections",
                SetId = "base1",
                Abilities = null,
                Attacks = null,
                Resistances = null,
                Weaknesses = null,
                CardMarket = null,
                Tcgplayer = null
            };

            var result = await repo.SaveCardAsync(updated, CancellationToken.None);

            Assert.Equal("nil-1", result.CardId);
        }

        [Fact]
        public async Task SaveCardAsync_ExistingCard_UpdatesExistingAbilityByName()
        {
            var db = CreateDb(nameof(SaveCardAsync_ExistingCard_UpdatesExistingAbilityByName));
            await SeedFullCard(db, "abn-1", "abn-ext-1");

            var repo = CreateRepo(db);
            var updated = new Card
            {
                CardId = "abn-1",
                ExternalId = "abn-ext-1",
                Name = "Pikachu",
                SetId = "base1",
                Abilities = new List<Ability>
                {
                    new() { Name = "Static", Text = "Updated text", Type = "Ability" }
                },
                Attacks = new List<Attack>
                {
                    new() { Name = "Thunder Shock", Text = "Updated attack text", Damage = "20", CostJson = "[\"Lightning\"]", ConvertedEnergyCost = "1" }
                },
                Resistances = new List<Resistance>
                {
                    new() { Type = "Fighting", Value = "-30" }
                },
                Weaknesses = new List<Weakness>
                {
                    new() { Type = "Ground", Value = "×2" }
                },
                CardMarket = new CardMarket
                {
                    Url = new Uri("https://cm.test"),
                    UpdatedAt = "2024-01-01",
                    CardMarketPrices = new List<CardMarketPrice>
                    {
                        new() { AverageSellPrice = 1.5m, LowPrice = 0.5m, TrendPrice = 1.0m }
                    }
                },
                Tcgplayer = new TCGPlayer
                {
                    Url = new Uri("https://tcg.test"),
                    UpdatedAt = "2024-01-01",
                    TCGPlayerPrices = new List<TCGPlayerPrice>
                    {
                        new()
                        {
                            Prices = new List<Price>
                            {
                                new() { Type = PriceType.Normal, Low = 0.5, Mid = 1.0, High = 2.0, Market = 1.0, DirectLow = 0.4 }
                            }
                        }
                    }
                }
            };

            var result = await repo.SaveCardAsync(updated, CancellationToken.None);

            Assert.Equal("abn-1", result.CardId);
        }

        [Fact]
        public async Task SaveCardAsync_CardWithNoImage_ReturnsNullImage()
        {
            var db = CreateDb(nameof(SaveCardAsync_CardWithNoImage_ReturnsNullImage));
            await SeedBaseSet(db);

            db.Cards.Add(new Card
            {
                CardId = "noimg-1",
                ExternalId = "noimg-ext-1",
                Name = "NoImage",
                SetId = "base1",
                CardImageId = null,
                CardMarket = new CardMarket { Url = null, CardMarketPrices = new List<CardMarketPrice>() },
                Tcgplayer = new TCGPlayer { Url = null, TCGPlayerPrices = new List<TCGPlayerPrice>() }
            });
            await db.SaveChangesAsync();

            var repo = CreateRepo(db);
            var result = await repo.GetCardsByCardIdAsync("noimg-1");

            Assert.Single(result);
            Assert.Null(result[0].ImageLarge);
        }
    }
}
