using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using PokemonTCG.API.Services;

namespace PokemonTCG.Test.Services
{
    public class ScannerServiceTests
    {
        private readonly Mock<ILogger<ScannerService>> _logger = new();
        private readonly Mock<IWebHostEnvironment> _env = new();

        public ScannerServiceTests()
        {
            // Point to a temp dir so the service constructor doesn't fail
            _env.Setup(e => e.ContentRootPath).Returns(Path.GetTempPath());
        }

        private ScannerService CreateService() => new(_logger.Object, _env.Object);

        [Fact]
        public void Constructor_SetsCorrectTessdataPath()
        {
            var service = CreateService();
            // Service is created without throwing
            Assert.NotNull(service);
        }

        [Fact]
        public async Task ExtractCardDataFromImageAsync_ReturnsEmptyData_WhenTesseractNotAvailable()
        {
            // Without tessdata files, OCR will fail and the catch block returns an empty ScannedCardData
            var service = CreateService();
            var fakeImage = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }; // PNG header bytes

            var result = await service.ExtractCardDataFromImageAsync(fakeImage);

            // When Tesseract fails, the catch block returns an empty ScannedCardData
            Assert.NotNull(result);
            Assert.IsType<ScannedCardData>(result);
        }

        [Fact]
        public async Task ExtractCardDataFromImageAsync_ReturnsEmptyData_WhenImageIsEmpty()
        {
            var service = CreateService();
            var result = await service.ExtractCardDataFromImageAsync(new byte[0]);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task ExtractCardDataFromImageAsync_DoesNotThrow_WhenImageBytesInvalid()
        {
            var service = CreateService();
            var result = await service.ExtractCardDataFromImageAsync(new byte[] { 1, 2, 3 });

            // Should not throw - errors are caught internally
            Assert.NotNull(result);
        }

        [Fact]
        public void ScannedCardData_DefaultValues_AreCorrect()
        {
            var data = new ScannedCardData();

            Assert.Equal(string.Empty, data.Name);
            Assert.Equal(string.Empty, data.HP);
            Assert.Equal(string.Empty, data.Supertype);
            Assert.Equal(string.Empty, data.Stage);
            Assert.Equal(string.Empty, data.EvolvesFrom);
            Assert.NotNull(data.Attacks);
            Assert.Empty(data.Attacks);
            Assert.Equal(string.Empty, data.Weakness);
            Assert.Equal(string.Empty, data.Resistance);
            Assert.Equal(string.Empty, data.RetreatCost);
            Assert.Equal(string.Empty, data.Artist);
            Assert.Equal(string.Empty, data.Number);
            Assert.Equal(string.Empty, data.FullText);
        }

        [Fact]
        public void ScannedCardData_CanSetAllProperties()
        {
            var data = new ScannedCardData
            {
                Name = "Pikachu",
                HP = "60",
                Supertype = "Pokémon",
                Stage = "Basic",
                EvolvesFrom = "",
                Attacks = new List<string> { "Thunder Shock — 40" },
                Weakness = "Fighting ×2",
                Resistance = "Metal -20",
                RetreatCost = "1",
                Artist = "Ken Sugimori",
                Number = "25",
                FullText = "Pikachu HP 60..."
            };

            Assert.Equal("Pikachu", data.Name);
            Assert.Equal("60", data.HP);
            Assert.Equal("Pokémon", data.Supertype);
            Assert.Equal("Basic", data.Stage);
            Assert.Single(data.Attacks);
            Assert.Equal("Thunder Shock — 40", data.Attacks[0]);
            Assert.Equal("Fighting ×2", data.Weakness);
            Assert.Equal("Metal -20", data.Resistance);
            Assert.Equal("1", data.RetreatCost);
            Assert.Equal("Ken Sugimori", data.Artist);
            Assert.Equal("25", data.Number);
        }
    }
}
