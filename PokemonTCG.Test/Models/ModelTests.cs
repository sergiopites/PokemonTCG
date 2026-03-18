using PokemonTCG.API.Models;

namespace PokemonTCG.Test.Models
{
    public class ModelTests
    {
        [Fact]
        public void Card_PropertiesSetCorrectly()
        {
            var card = new Card
            {
                CardId = "xy1-1",
                ExternalId = "xy1-1",
                Name = "Pikachu",
                SuperType = "Pokémon",
                SubTypes = "Basic",
                Hp = 60,
                Types = "Lightning",
                Number = "35",
                Rarity = "Common",
                Artist = "Ken Sugimori",
                SetId = "xy1"
            };

            Assert.Equal("xy1-1", card.CardId);
            Assert.Equal("Pikachu", card.Name);
            Assert.Equal(60, card.Hp);
        }

        [Fact]
        public void Card_DeckCards_DefaultsToEmptyList()
        {
            var card = new Card { CardId = "xy1-1", ExternalId = "xy1-1" };

            Assert.NotNull(card.DeckCards);
            Assert.Empty(card.DeckCards);
        }

        [Fact]
        public void Set_PropertiesSetCorrectly()
        {
            var set = new Set
            {
                SetId = "xy1",
                Name = "XY",
                Series = "XY",
                PrintedTotal = 146,
                Total = 146,
                PtcgoCode = "XY",
                ReleaseDate = "2014/02/05"
            };

            Assert.Equal("xy1", set.SetId);
            Assert.Equal(146, set.PrintedTotal);
        }

        [Fact]
        public void CardImage_PropertiesSetCorrectly()
        {
            var img = new CardImage
            {
                CardImageId = 1,
                Small = new Uri("https://example.com/small.png"),
                Large = new Uri("https://example.com/large.png")
            };

            Assert.Equal(1, img.CardImageId);
            Assert.NotNull(img.Small);
        }

        [Fact]
        public void Legality_PropertiesSetCorrectly()
        {
            var legality = new Legality
            {
                LegalityId = 1,
                Standard = "Legal",
                Expanded = "Legal",
                Unlimited = "Legal"
            };

            Assert.Equal("Legal", legality.Standard);
        }

        [Fact]
        public void Ability_PropertiesSetCorrectly()
        {
            var ability = new Ability
            {
                AbilityId = 1,
                Name = "Static",
                Text = "Paralyze on contact",
                Type = "Ability",
                CardId = "xy1-1"
            };

            Assert.Equal("Static", ability.Name);
        }

        [Fact]
        public void Attack_PropertiesSetCorrectly()
        {
            var attack = new Attack
            {
                AttackId = 1,
                Name = "Thunder Shock",
                Damage = "30",
                Text = "Flip a coin",
                CostJson = "[\"Lightning\"]",
                ConvertedEnergyCost = "1",
                CardId = "xy1-1"
            };

            Assert.Equal("Thunder Shock", attack.Name);
        }

        [Fact]
        public void Weakness_PropertiesSetCorrectly()
        {
            var weakness = new Weakness
            {
                WeaknessId = 1,
                Type = "Fighting",
                Value = "×2",
                CardId = "xy1-1"
            };

            Assert.Equal("Fighting", weakness.Type);
        }

        [Fact]
        public void Resistance_PropertiesSetCorrectly()
        {
            var resistance = new Resistance
            {
                ResistanceId = 1,
                Type = "Metal",
                Value = "-20",
                CardId = "xy1-1"
            };

            Assert.Equal("Metal", resistance.Type);
        }

        [Fact]
        public void SetImage_PropertiesSetCorrectly()
        {
            var img = new SetImage
            {
                SetImageId = 1,
                Logo = new Uri("https://example.com/logo.png"),
                Symbol = new Uri("https://example.com/symbol.png")
            };

            Assert.Equal(1, img.SetImageId);
            Assert.NotNull(img.Logo);
        }

        [Fact]
        public void AncientTrait_PropertiesSetCorrectly()
        {
            var trait = new AncientTrait
            {
                AncientTraitId = 1,
                Name = "α Growth",
                Text = "Attach 2 Energy"
            };

            Assert.Equal("α Growth", trait.Name);
        }

        [Fact]
        public void Deck_PropertiesSetCorrectly()
        {
            var deck = new Deck
            {
                DeckId = 1,
                Name = "Fire Deck",
                Description = "A fire-type deck"
            };

            Assert.Equal(1, deck.DeckId);
            Assert.NotNull(deck.DeckCards);
        }

        [Fact]
        public void DeckCard_PropertiesSetCorrectly()
        {
            var dc = new DeckCard
            {
                DeckCardId = 1,
                DeckId = 1,
                CardId = "xy1-1",
                Quantity = 4
            };

            Assert.Equal(4, dc.Quantity);
        }

        [Fact]
        public void CardMarket_PropertiesSetCorrectly()
        {
            var cm = new CardMarket
            {
                CardMarketId = 1,
                Url = new Uri("https://cardmarket.com"),
                UpdatedAt = "2024/01/01",
                CardId = "xy1-1"
            };

            Assert.Equal(1, cm.CardMarketId);
        }

        [Fact]
        public void TCGPlayer_PropertiesSetCorrectly()
        {
            var tp = new TCGPlayer
            {
                TCGPlayerId = 1,
                Url = new Uri("https://tcgplayer.com"),
                UpdatedAt = "2024/01/01",
                CardId = "xy1-1"
            };

            Assert.Equal(1, tp.TCGPlayerId);
        }

        [Fact]
        public void CardMarketPrice_PropertiesSetCorrectly()
        {
            var cmp = new CardMarketPrice
            {
                CardMarketPriceId = 1,
                CardMarketId = 1,
                AverageSellPrice = 5.99m,
                LowPrice = 2.50m,
                TrendPrice = 6.00m,
                ReverseHoloLow = 3.00m,
                ReverseHoloTrend = 4.00m,
                LowPriceExPlus = 3.50m,
                AverageDay = 5.50m,
                AverageWeek = 5.75m,
                AverageMonth = 5.80m,
                AverageDayReverseHolo = 3.50m,
                AverageWeekReverseHolo = 3.60m,
                AverageMonthReverseHolo = 3.70m
            };

            Assert.Equal(5.99m, cmp.AverageSellPrice);
        }

        [Fact]
        public void TCGPlayerPrice_PropertiesSetCorrectly()
        {
            var tpp = new TCGPlayerPrice
            {
                TCGPlayerPriceId = 1,
                TCGPlayerId = 1,
                Prices = new List<Price>
                {
                    new Price
                    {
                        PriceId = 1,
                        Type = PriceType.Holofoil,
                        Low = 1.0,
                        Mid = 2.0,
                        High = 5.0,
                        Market = 3.0,
                        DirectLow = 1.5
                    }
                }
            };

            Assert.Single(tpp.Prices);
            Assert.Equal(PriceType.Holofoil, tpp.Prices.First().Type);
        }

        [Fact]
        public void Price_PropertiesSetCorrectly()
        {
            var price = new Price
            {
                PriceId = 1,
                TCGPlayerPriceId = 1,
                Type = PriceType.Normal,
                Low = 0.5,
                Mid = 1.0,
                High = 3.0,
                Market = 1.5,
                DirectLow = 0.8
            };

            Assert.Equal(PriceType.Normal, price.Type);
            Assert.Equal(0.5, price.Low);
        }

        [Fact]
        public void PriceType_HasExpectedValues()
        {
            Assert.Equal(1, (int)PriceType.Holofoil);
            Assert.Equal(2, (int)PriceType.ReverseHolofoil);
            Assert.Equal(3, (int)PriceType.Normal);
            Assert.Equal(4, (int)PriceType.FirstEditionHolofoil);
            Assert.Equal(5, (int)PriceType.UnlimitedHolofoil);
            Assert.Equal(6, (int)PriceType.FirstEdition);
            Assert.Equal(7, (int)PriceType.Unlimited);
        }
    }
}