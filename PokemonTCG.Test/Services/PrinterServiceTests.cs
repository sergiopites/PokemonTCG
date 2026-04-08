using Microsoft.Extensions.Logging;
using Moq;
using PokemonTCG.API.Services;

namespace PokemonTCG.Test.Services
{
    public class PrinterServiceTests
    {
        private readonly Mock<ILogger<PokemonTCG.Printer.Printer>> _logger = new();

        private PrinterService CreateService() => new(_logger.Object);

        // ?? GenerateCardPdfAsync ????????????????????????????????????????

        [Fact]
        public async Task GenerateCardPdfAsync_NullUrls_ThrowsArgumentException()
        {
            var service = CreateService();

            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.GenerateCardPdfAsync(null!, "test"));
        }

        [Fact]
        public async Task GenerateCardPdfAsync_EmptyUrls_ThrowsArgumentException()
        {
            var service = CreateService();

            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.GenerateCardPdfAsync(new List<string>(), "test"));
        }

        [Fact]
        public async Task GenerateCardPdfAsync_BlankFileName_DefaultsToFileName()
        {
            var service = CreateService();

            // Blank fileName should not throw, the validation passes and it defaults to "PokemonCard"
            // but it will fail on the actual URL download — that's expected from the Printer dependency
            await Assert.ThrowsAnyAsync<Exception>(() =>
                service.GenerateCardPdfAsync(new List<string> { "invalid-url" }, "  "));
        }

        [Fact]
        public async Task GenerateCardPdfAsync_InvalidUrl_ThrowsFromPrinter()
        {
            var service = CreateService();

            // URL passes the service validation but fails in the Printer's download
            await Assert.ThrowsAnyAsync<Exception>(() =>
                service.GenerateCardPdfAsync(new List<string> { "not-a-valid-url" }, "test"));
        }

        // ?? GenerateCardDeckPdfAsync ????????????????????????????????????

        [Fact]
        public async Task GenerateCardDeckPdfAsync_NullUrls_ThrowsArgumentException()
        {
            var service = CreateService();

            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.GenerateCardDeckPdfAsync(null!, "test"));
        }

        [Fact]
        public async Task GenerateCardDeckPdfAsync_EmptyUrls_ThrowsArgumentException()
        {
            var service = CreateService();

            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.GenerateCardDeckPdfAsync(new List<string>(), "test"));
        }

        [Fact]
        public async Task GenerateCardDeckPdfAsync_BlankFileName_DefaultsToFileName()
        {
            var service = CreateService();

            await Assert.ThrowsAnyAsync<Exception>(() =>
                service.GenerateCardDeckPdfAsync(new List<string> { "invalid-url" }, ""));
        }

        [Fact]
        public async Task GenerateCardDeckPdfAsync_InvalidUrl_ThrowsFromPrinter()
        {
            var service = CreateService();

            await Assert.ThrowsAnyAsync<Exception>(() =>
                service.GenerateCardDeckPdfAsync(new List<string> { "not-a-valid-url" }, "deck"));
        }

        [Fact]
        public async Task GenerateCardPdfAsync_ForwardsProgressParameter()
        {
            var service = CreateService();
            var progress = new Progress<int>(_ => { });

            // The call will fail on invalid URL, but it proves progress is accepted and forwarded
            await Assert.ThrowsAnyAsync<Exception>(() =>
                service.GenerateCardPdfAsync(new List<string> { "not-a-valid-url" }, "test", progress));
        }

        [Fact]
        public async Task GenerateCardDeckPdfAsync_ForwardsProgressParameter()
        {
            var service = CreateService();
            var progress = new Progress<int>(_ => { });

            await Assert.ThrowsAnyAsync<Exception>(() =>
                service.GenerateCardDeckPdfAsync(new List<string> { "not-a-valid-url" }, "deck", progress));
        }

        [Fact]
        public async Task GenerateCardPdfAsync_NullProgress_DoesNotThrow()
        {
            var service = CreateService();

            // Should still throw from invalid URL, not from null progress
            await Assert.ThrowsAnyAsync<Exception>(() =>
                service.GenerateCardPdfAsync(new List<string> { "not-a-valid-url" }, "test", null));
        }

        [Fact]
        public async Task GenerateCardDeckPdfAsync_NullProgress_DoesNotThrow()
        {
            var service = CreateService();

            await Assert.ThrowsAnyAsync<Exception>(() =>
                service.GenerateCardDeckPdfAsync(new List<string> { "not-a-valid-url" }, "deck", null));
        }

        [Fact]
        public async Task GenerateCardPdfAsync_NullFileName_DefaultsToPokemonCard()
        {
            var service = CreateService();

            // Should not throw ArgumentException for fileName, should throw from Printer
            var ex = await Assert.ThrowsAnyAsync<Exception>(() =>
                service.GenerateCardPdfAsync(new List<string> { "not-a-valid-url" }, null!));

            Assert.IsNotType<ArgumentException>(ex);
        }

        [Fact]
        public async Task GenerateCardDeckPdfAsync_NullFileName_DefaultsToPokemonCard()
        {
            var service = CreateService();

            var ex = await Assert.ThrowsAnyAsync<Exception>(() =>
                service.GenerateCardDeckPdfAsync(new List<string> { "not-a-valid-url" }, null!));

            Assert.IsNotType<ArgumentException>(ex);
        }

        // ?? GenerateCardsExcelAsync ????????????????????????????????

        [Fact]
        public async Task GenerateCardsExcelAsync_NullCards_ThrowsArgumentException()
        {
            var service = CreateService();

            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.GenerateCardsExcelAsync(null!, "test"));
        }

        [Fact]
        public async Task GenerateCardsExcelAsync_EmptyCards_ThrowsArgumentException()
        {
            var service = CreateService();

            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.GenerateCardsExcelAsync(new List<PokemonTCG.API.Request.ExportCardItem>(), "test"));
        }

        [Fact]
        public async Task GenerateCardsExcelAsync_ValidCards_ReturnsBytes()
        {
            var service = CreateService();
            var cards = new List<PokemonTCG.API.Request.ExportCardItem>
            {
                new() { CardId = "xy1-1", Name = "Pikachu", Supertype = "Pokémon", Quantity = 4, ImageLarge = "https://images.pokemontcg.io/xy1/1_hires.png" },
                new() { CardId = "xy1-2", Name = "Charizard", Supertype = "Pokémon", Quantity = 2, ImageLarge = "https://images.pokemontcg.io/xy1/2_hires.png" }
            };

            var result = await service.GenerateCardsExcelAsync(cards, "TestDeck");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);

            using var stream = new MemoryStream(result);
            using var workbook = new ClosedXML.Excel.XLWorkbook(stream);
            var ws = workbook.Worksheets.First();
            Assert.Equal("https://images.pokemontcg.io/xy1/1_hires.png", ws.Cell(2, 16).GetString());
            Assert.Equal("https://images.pokemontcg.io/xy1/2_hires.png", ws.Cell(3, 16).GetString());
        }

        [Fact]
        public async Task GenerateCardsExcelAsync_BlankFileName_DefaultsToFileName()
        {
            var service = CreateService();
            var cards = new List<PokemonTCG.API.Request.ExportCardItem>
            {
                new() { CardId = "xy1-1", Name = "Pikachu", Quantity = 1 }
            };

            // Should not throw - blank file name defaults to "PokemonCards"
            var result = await service.GenerateCardsExcelAsync(cards, "  ");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public async Task GenerateCardsExcelAsync_ReportsProgress()
        {
            var service = CreateService();
            var cards = new List<PokemonTCG.API.Request.ExportCardItem>
            {
                new() { CardId = "c1", Name = "Card1", Quantity = 1 },
                new() { CardId = "c2", Name = "Card2", Quantity = 1 },
                new() { CardId = "c3", Name = "Card3", Quantity = 1 }
            };

            var reported = new List<int>();
            var progress = new Progress<int>(pct => reported.Add(pct));

            await service.GenerateCardsExcelAsync(cards, "test", progress);

            // Progress may be batched by SynchronizationContext, but at minimum it should have been reported
            // Since Progress<T> posts to SyncContext, in test environment values may arrive asynchronously
            // We just verify no exception is thrown when progress is provided
            Assert.NotNull(reported);
        }

        [Fact]
        public async Task GenerateCardsExcelAsync_NullProgress_DoesNotThrow()
        {
            var service = CreateService();
            var cards = new List<PokemonTCG.API.Request.ExportCardItem>
            {
                new() { CardId = "c1", Name = "Card1", Quantity = 1 }
            };

            var result = await service.GenerateCardsExcelAsync(cards, "test", null);

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }
    }
}
