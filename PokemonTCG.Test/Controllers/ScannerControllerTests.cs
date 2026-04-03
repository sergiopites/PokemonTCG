using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PokemonTCG.API.Controllers;
using PokemonTCG.API.DTOs;
using PokemonTCG.API.Helpers;
using PokemonTCG.API.Services;

namespace PokemonTCG.Test.Controllers
{
    public class ScannerControllerTests
    {
        private readonly Mock<IScannerService> _mockScanner;
        private readonly Mock<ICardService> _mockCardService;
        private readonly Mock<ILogger<ScannerController>> _mockLogger;
        private readonly ScannerController _controller;

        public ScannerControllerTests()
        {
            _mockScanner = new Mock<IScannerService>();
            _mockCardService = new Mock<ICardService>();
            _mockLogger = new Mock<ILogger<ScannerController>>();
            _controller = new ScannerController(_mockScanner.Object, _mockCardService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task ScanCard_ReturnsBadRequest_WhenRequestIsNull()
        {
            var result = await _controller.ScanCard(null!);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task ScanCard_ReturnsBadRequest_WhenImageBase64IsEmpty()
        {
            var result = await _controller.ScanCard(new ScanRequest { ImageBase64 = "" });
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task ScanCard_ReturnsBadRequest_WhenImageBase64IsWhitespace()
        {
            var result = await _controller.ScanCard(new ScanRequest { ImageBase64 = "   " });
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task ScanCard_ReturnsBadRequest_WhenBase64IsInvalid()
        {
            var result = await _controller.ScanCard(new ScanRequest { ImageBase64 = "not-valid-base64!!!" });
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task ScanCard_StripsDataUriPrefix_BeforeDecoding()
        {
            // "data:image/png;base64,AAAA" ? should strip prefix to "AAAA"
            // AAAA is valid base64 for 3 zero bytes
            byte[]? capturedBytes = null;
            _mockScanner.Setup(s => s.ExtractCardDataFromImageAsync(It.IsAny<byte[]>()))
                .Callback<byte[]>(b => capturedBytes = b)
                .ReturnsAsync(new ScannedCardData { Name = "Test" });

            _mockCardService.Setup(s => s.SearchCardsAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(),
                    It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(new PagedResult<CardDetailDTO> { Items = new List<CardDetailDTO> { new() { CardId = "c1" } }, TotalCount = 1 });

            await _controller.ScanCard(new ScanRequest { ImageBase64 = "data:image/png;base64,AAAA" });

            Assert.NotNull(capturedBytes);
            Assert.Equal(new byte[] { 0, 0, 0 }, capturedBytes);
        }

        [Fact]
        public async Task ScanCard_ReturnsOk_WhenNoNameAndNoNumber()
        {
            _mockScanner.Setup(s => s.ExtractCardDataFromImageAsync(It.IsAny<byte[]>()))
                .ReturnsAsync(new ScannedCardData { Name = "", Number = "" });

            // "AAAA" is valid base64
            var result = await _controller.ScanCard(new ScanRequest { ImageBase64 = "AAAA" });

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ScanResponse>(okResult.Value);
            Assert.Contains("Could not read", response.Message);
            Assert.Null(response.Cards);
        }        

        [Fact]
        public async Task ScanCard_ReturnsOk_WhenNoCardsFound()
        {
            _mockScanner.Setup(s => s.ExtractCardDataFromImageAsync(It.IsAny<byte[]>()))
                .ReturnsAsync(new ScannedCardData { Name = "Unknown", Number = "999" });

            _mockCardService.Setup(s => s.SearchCardsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(new PagedResult<CardDetailDTO> { Items = new List<CardDetailDTO>(), TotalCount = 0 });

            var result = await _controller.ScanCard(new ScanRequest { ImageBase64 = "AAAA" });

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ScanResponse>(okResult.Value);
            Assert.Contains("No cards found", response.Message);
        }

        [Fact]
        public async Task ScanCard_Returns500_WhenScannerThrows()
        {
            _mockScanner.Setup(s => s.ExtractCardDataFromImageAsync(It.IsAny<byte[]>()))
                .ThrowsAsync(new Exception("OCR failed"));

            var result = await _controller.ScanCard(new ScanRequest { ImageBase64 = "AAAA" });

            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusResult.StatusCode);
        }
    }
}
