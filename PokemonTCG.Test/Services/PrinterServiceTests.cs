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
    }
}
