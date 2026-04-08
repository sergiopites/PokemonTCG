using Microsoft.Extensions.Logging;
using Moq;
using PokemonTCG.API.DTOs;
using PokemonTCG.API.Helpers;
using PokemonTCG.API.Repositories;
using PokemonTCG.API.Request;
using PokemonTCG.API.Services;

namespace PokemonTCG.Test.Services
{
    public class DeckServiceTests
    {
        private readonly Mock<IDeckRepository> _deckRepo = new();
        private readonly Mock<ICardRepository> _cardRepo = new();
        private readonly Mock<ILogger<IDeckRepository>> _logger = new();

        private DeckService CreateService() => new(_logger.Object, _deckRepo.Object, _cardRepo.Object);

        // ?? SaveDeckAsync — validation ??????????????????????????????

        [Fact]
        public async Task SaveDeckAsync_Throws_WhenTotalIsNot60()
        {
            var request = new DeckRequest
            {
                Name = "Bad",
                Description = "Wrong total",
                Cards = new List<CardQuantityRequest>
                {
                    new() { CardId = "c1", Quantity = 10, Supertype = "Pokémon" }
                }
            };

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                CreateService().SaveDeckAsync(request));
            Assert.Contains("exactly 60", ex.Message);
        }

        [Fact]
        public async Task SaveDeckAsync_Throws_WhenNonEnergyExceeds4Copies()
        {
            var cards = new List<CardQuantityRequest>
            {
                new() { CardId = "c1", Quantity = 5, Supertype = "Pokémon", Name = "Pikachu" },
                new() { CardId = "c2", Quantity = 55, Supertype = "Energy" }
            };
            var request = new DeckRequest { Name = "Bad", Description = "Over limit", Cards = cards };

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                CreateService().SaveDeckAsync(request));
            Assert.Contains("4-copy limit", ex.Message);
            Assert.Contains("Pikachu", ex.Message);
        }

        [Fact]
        public async Task SaveDeckAsync_AllowsEnergyOver4Copies()
        {
            var cards = new List<CardQuantityRequest>
            {
                new() { CardId = "pika-1", Quantity = 4, Supertype = "Pokémon" },
                new() { CardId = "energy-fire", Quantity = 56, Supertype = "Energy" }
            };
            var request = new DeckRequest { Name = "Energy Heavy", Description = "Test", Cards = cards };

            _deckRepo.Setup(r => r.SaveDeckAsync(It.IsAny<DeckDetailDTO>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((DeckDetailDTO dto, CancellationToken _) => { dto.DeckId = 1; return dto; });

            var result = await CreateService().SaveDeckAsync(request);

            Assert.Equal(1, result.DeckId);
        }

        [Fact]
        public async Task SaveDeckAsync_PassesSupertypeToDTO()
        {
            var cards = new List<CardQuantityRequest>
            {
                new() { CardId = "pika-1", Quantity = 4, Supertype = "Pokémon" },
                new() { CardId = "energy-1", Quantity = 56, Supertype = "Energy" }
            };
            var request = new DeckRequest { Name = "Test", Description = "Desc", Cards = cards };

            DeckDetailDTO? capturedDto = null;
            _deckRepo.Setup(r => r.SaveDeckAsync(It.IsAny<DeckDetailDTO>(), It.IsAny<CancellationToken>()))
                .Callback<DeckDetailDTO, CancellationToken>((dto, _) => capturedDto = dto)
                .ReturnsAsync((DeckDetailDTO dto, CancellationToken _) => dto);

            await CreateService().SaveDeckAsync(request);

            Assert.NotNull(capturedDto);
            Assert.Equal("Pokémon", capturedDto!.Cards[0].Supertype);
            Assert.Equal("Energy", capturedDto.Cards[1].Supertype);
        }

        // ?? SaveDeckAsync — mapping ?????????????????????????????????

        [Fact]
        public async Task SaveDeckAsync_ValidRequest_SavesAndReturns()
        {
            var request = Build60CardRequest("My Deck", "A test deck");

            _deckRepo.Setup(r => r.SaveDeckAsync(It.IsAny<DeckDetailDTO>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((DeckDetailDTO dto, CancellationToken _) =>
                {
                    dto.DeckId = 42;
                    return dto;
                });

            var result = await CreateService().SaveDeckAsync(request);

            Assert.Equal(42, result.DeckId);
            Assert.Equal("My Deck", result.Name);
        }

        [Fact]
        public async Task SaveDeckAsync_MapsCardsCorrectly()
        {
            var request = new DeckRequest
            {
                Name = "Deck",
                Description = "Desc",
                Cards = new List<CardQuantityRequest>
                {
                    new() { CardId = "char-1", Quantity = 3, Supertype = "Pokémon" },
                    new() { CardId = "squirtle-1", Quantity = 1, Supertype = "Pokémon" },
                    new() { CardId = "energy-1", Quantity = 56, Supertype = "Energy" }
                }
            };

            _deckRepo.Setup(r => r.SaveDeckAsync(It.IsAny<DeckDetailDTO>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((DeckDetailDTO dto, CancellationToken _) => dto);

            var result = await CreateService().SaveDeckAsync(request);

            Assert.Equal(3, result.Cards.Count);
            Assert.Equal("char-1", result.Cards.First().CardId);
            Assert.Equal(3, result.Cards.First().Quantity);
        }

        [Fact]
        public async Task SaveDeckAsync_PassesDeckIdFromRequest()
        {
            var request = Build60CardRequest("Update", "Updating");
            request.DeckId = 99;

            DeckDetailDTO? capturedDto = null;
            _deckRepo.Setup(r => r.SaveDeckAsync(It.IsAny<DeckDetailDTO>(), It.IsAny<CancellationToken>()))
                .Callback<DeckDetailDTO, CancellationToken>((dto, _) => capturedDto = dto)
                .ReturnsAsync((DeckDetailDTO dto, CancellationToken _) => dto);

            await CreateService().SaveDeckAsync(request);

            Assert.NotNull(capturedDto);
            Assert.Equal(99, capturedDto!.DeckId);
        }

        // ?? GetAllDecksAsync ????????????????????????????????????????

        [Fact]
        public async Task GetAllDecksAsync_ReturnsAllDecks()
        {
            var dtos = new List<DeckDetailDTO>
            {
                new() { DeckId = 1, Name = "D1", Description = "Desc1", Cards = new List<DeckCardDTO> { new() { CardId = "c1", Quantity = 4 } } },
                new() { DeckId = 2, Name = "D2", Description = "Desc2", Cards = new List<DeckCardDTO>() }
            };
            _deckRepo.Setup(r => r.GetAllDecksAsync(It.IsAny<CancellationToken>())).ReturnsAsync(dtos);

            var result = await CreateService().GetAllDecksAsync();

            Assert.Equal(2, result.Count);
            Assert.Equal("D1", result[0].Name);
            Assert.Single(result[0].Cards);
            Assert.Empty(result[1].Cards);
        }

        [Fact]
        public async Task GetAllDecksAsync_EmptyList_ReturnsEmpty()
        {
            _deckRepo.Setup(r => r.GetAllDecksAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<DeckDetailDTO>());

            var result = await CreateService().GetAllDecksAsync();

            Assert.Empty(result);
        }

        // ?? GetDeckByIdAsync ????????????????????????????????????????

        [Fact]
        public async Task GetDeckByIdAsync_ReturnsDeck_WhenFound()
        {
            var dto = new DeckDetailDTO
            {
                DeckId = 1, Name = "Deck", Description = "Desc",
                Cards = new List<DeckCardDTO>
                {
                    new() { CardId = "xy1-1", Quantity = 2, Name = "Pikachu", ImageLarge = "https://img/pika.png", Supertype = "Pokémon", Subtype = "Basic", Number = "25", Ptcgocode = "XY" }
                }
            };
            _deckRepo.Setup(r => r.GetDeckByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(dto);

            var result = await CreateService().GetDeckByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal(1, result!.DeckId);
            Assert.Equal("Deck", result.Name);
            var card = result.Cards.First();
            Assert.Equal("xy1-1", card.CardId);
            Assert.Equal("Pikachu", card.Name);
            Assert.Equal("https://img/pika.png", card.ImageLarge);
            Assert.Equal("Pokémon", card.Supertype);
            Assert.Equal("Basic", card.Subtype);
            Assert.Equal("25", card.Number);
            Assert.Equal("XY", card.Ptcgocode);
        }

        [Fact]
        public async Task GetDeckByIdAsync_ReturnsNull_WhenNotFound()
        {
            _deckRepo.Setup(r => r.GetDeckByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((DeckDetailDTO?)null);

            var result = await CreateService().GetDeckByIdAsync(999);

            Assert.Null(result);
        }

        // ?? GenerateAutoDeckAsync ??????????????????????????????????

        [Fact]
        public async Task GenerateAutoDeckAsync_Throws_WhenNoPokemonCards()
        {
            _cardRepo.Setup(r => r.SearchCardsAsync(null, null, null, "Pokémon", null, null, null, 1, 2000, null))
                .ReturnsAsync(new PagedResult<CardDetailDTO> { Items = new List<CardDetailDTO>(), TotalCount = 0 });
            _cardRepo.Setup(r => r.SearchCardsAsync(null, null, null, "Trainer", null, null, null, 1, 2000, null))
                .ReturnsAsync(new PagedResult<CardDetailDTO> { Items = new List<CardDetailDTO>(), TotalCount = 0 });
            _cardRepo.Setup(r => r.SearchCardsAsync(null, null, null, "Energy", null, null, null, 1, 1000, null))
                .ReturnsAsync(new PagedResult<CardDetailDTO> { Items = new List<CardDetailDTO>(), TotalCount = 0 });

            await Assert.ThrowsAsync<Exception>(() => CreateService().GenerateAutoDeckAsync());
        }

        [Fact]
        public async Task GenerateAutoDeckAsync_ReturnsExactly60Cards()
        {
            var pokemon = Enumerable.Range(1, 50).Select(i => new CardDetailDTO
            {
                CardId = $"poke-{i}", Name = $"Pokemon{i}", Supertype = "Pokémon",
                Subtype = "Basic", Type = "Fire", SetId = "set1", Number = i.ToString()
            }).ToList();

            var trainers = Enumerable.Range(1, 50).Select(i => new CardDetailDTO
            {
                CardId = $"trainer-{i}", Name = $"Trainer{i}", Supertype = "Trainer",
                Subtype = "Item", SetId = "set1", Number = i.ToString()
            }).ToList();

            var energies = Enumerable.Range(1, 20).Select(i => new CardDetailDTO
            {
                CardId = $"energy-{i}", Name = $"Fire Energy", Supertype = "Energy",
                Subtype = "Basic", Type = "Fire", SetId = "set1", Number = i.ToString()
            }).ToList();

            _cardRepo.Setup(r => r.SearchCardsAsync(null, null, null, "Pokémon", null, null, null, 1, 2000, null))
                .ReturnsAsync(new PagedResult<CardDetailDTO> { Items = pokemon, TotalCount = pokemon.Count });
            _cardRepo.Setup(r => r.SearchCardsAsync(null, null, null, "Trainer", null, null, null, 1, 2000, null))
                .ReturnsAsync(new PagedResult<CardDetailDTO> { Items = trainers, TotalCount = trainers.Count });
            _cardRepo.Setup(r => r.SearchCardsAsync(null, null, null, "Energy", null, null, null, 1, 1000, null))
                .ReturnsAsync(new PagedResult<CardDetailDTO> { Items = energies, TotalCount = energies.Count });

            var result = await CreateService().GenerateAutoDeckAsync();

            Assert.Equal(60, result.Total);
            Assert.Equal(60, result.Cards.Count);
            Assert.NotNull(result.DominantType);
            Assert.NotNull(result.DeckName);
        }

        [Fact]
        public async Task GenerateAutoDeckAsync_HasAtLeastOnePokemonTrainerAndEnergy()
        {
            var pokemon = Enumerable.Range(1, 50).Select(i => new CardDetailDTO
            {
                CardId = $"poke-{i}", Name = $"Pokemon{i}", Supertype = "Pokémon",
                Subtype = "Basic", Type = "Water", SetId = "set1", Number = i.ToString()
            }).ToList();

            var trainers = Enumerable.Range(1, 50).Select(i => new CardDetailDTO
            {
                CardId = $"trainer-{i}", Name = $"Trainer{i}", Supertype = "Trainer",
                Subtype = "Supporter", SetId = "set1", Number = i.ToString()
            }).ToList();

            var energies = Enumerable.Range(1, 20).Select(i => new CardDetailDTO
            {
                CardId = $"energy-{i}", Name = $"Water Energy", Supertype = "Energy",
                Subtype = "Basic", Type = "Water", SetId = "set1", Number = i.ToString()
            }).ToList();

            _cardRepo.Setup(r => r.SearchCardsAsync(null, null, null, "Pokémon", null, null, null, 1, 2000, null))
                .ReturnsAsync(new PagedResult<CardDetailDTO> { Items = pokemon, TotalCount = pokemon.Count });
            _cardRepo.Setup(r => r.SearchCardsAsync(null, null, null, "Trainer", null, null, null, 1, 2000, null))
                .ReturnsAsync(new PagedResult<CardDetailDTO> { Items = trainers, TotalCount = trainers.Count });
            _cardRepo.Setup(r => r.SearchCardsAsync(null, null, null, "Energy", null, null, null, 1, 1000, null))
                .ReturnsAsync(new PagedResult<CardDetailDTO> { Items = energies, TotalCount = energies.Count });

            var result = await CreateService().GenerateAutoDeckAsync();

            Assert.True(result.Pokémon > 0, "Should have at least 1 Pokémon");
            Assert.True(result.Trainers > 0, "Should have at least 1 Trainer");
            Assert.True(result.Energy > 0, "Should have at least 1 Energy");
            Assert.Equal(60, result.Pokémon + result.Trainers + result.Energy);
        }

        private static DeckRequest Build60CardRequest(string name, string description)
        {
            return new DeckRequest
            {
                Name = name,
                Description = description,
                Cards = new List<CardQuantityRequest>
                {
                    new() { CardId = "pika-1", Quantity = 4, Supertype = "Pokémon" },
                    new() { CardId = "energy-1", Quantity = 56, Supertype = "Energy" }
                }
            };
        }
    }
}
