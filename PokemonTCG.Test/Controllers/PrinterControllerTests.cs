using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PokemonTCG.API.Controllers;
using PokemonTCG.API.Request;
using PokemonTCG.API.Services;

namespace PokemonTCG.Test.Controllers
{
    public class PrinterControllerTests
    {
        private readonly Mock<IPrinterService> _mockPrinterService;
        private readonly Mock<ILogger<PrinterController>> _mockLogger;
        private readonly PrinterController _controller;

        public PrinterControllerTests()
        {
            _mockPrinterService = new Mock<IPrinterService>();
            _mockLogger = new Mock<ILogger<PrinterController>>();
            _controller = new PrinterController(_mockPrinterService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task Generate_ReturnsFileResult()
        {
            var pdfBytes = new byte[] { 1, 2, 3 };
            _mockPrinterService.Setup(s => s.GenerateCardPdfAsync(It.IsAny<List<string>>(), "TestCard", It.IsAny<IProgress<int>>()))
                .ReturnsAsync(pdfBytes);

            var request = new PrintCardRequest { ImageUrls = new List<string> { "http://img.com/1.png" }, FileName = "TestCard" };
            var result = await _controller.Generate(request);

            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/pdf", fileResult.ContentType);
            Assert.Equal("TestCard.pdf", fileResult.FileDownloadName);
            Assert.Equal(pdfBytes, fileResult.FileContents);
        }

        [Fact]
        public async Task Generate_UsesDefaultFileName_WhenBlank()
        {
            var pdfBytes = new byte[] { 1, 2, 3 };
            _mockPrinterService.Setup(s => s.GenerateCardPdfAsync(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<IProgress<int>>()))
                .ReturnsAsync(pdfBytes);

            var request = new PrintCardRequest { ImageUrls = new List<string> { "http://img.com/1.png" }, FileName = "  " };
            var result = await _controller.Generate(request);

            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("PokemonCard.pdf", fileResult.FileDownloadName);
        }

        [Fact]
        public async Task GenerateDeck_ReturnsFileResult()
        {
            var pdfBytes = new byte[] { 4, 5, 6 };
            _mockPrinterService.Setup(s => s.GenerateCardDeckPdfAsync(It.IsAny<List<string>>(), "MyDeck", It.IsAny<IProgress<int>>()))
                .ReturnsAsync(pdfBytes);

            var request = new PrintCardRequest { ImageUrls = new List<string> { "http://img.com/1.png" }, FileName = "MyDeck" };
            var result = await _controller.GenerateDeck(request);

            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/pdf", fileResult.ContentType);
            Assert.Equal("MyDeck.pdf", fileResult.FileDownloadName);
            Assert.Equal(pdfBytes, fileResult.FileContents);
        }

        [Fact]
        public async Task GenerateDeck_UsesDefaultFileName_WhenEmpty()
        {
            var pdfBytes = new byte[] { 7, 8 };
            _mockPrinterService.Setup(s => s.GenerateCardDeckPdfAsync(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<IProgress<int>>()))
                .ReturnsAsync(pdfBytes);

            var request = new PrintCardRequest { ImageUrls = new List<string> { "http://img.com/1.png" }, FileName = "" };
            var result = await _controller.GenerateDeck(request);

            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("PokemonCard.pdf", fileResult.FileDownloadName);
        }

        [Fact]
        public async Task Generate_ReturnsBadRequest_WhenServiceThrows()
        {
            _mockPrinterService.Setup(s => s.GenerateCardPdfAsync(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<IProgress<int>>()))
                .ThrowsAsync(new ArgumentException("Image URLs cannot be empty."));

            var request = new PrintCardRequest { ImageUrls = new List<string>(), FileName = "Test" };
            var result = await _controller.Generate(request);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Image URLs cannot be empty.", badRequest.Value);
        }

        [Fact]
        public async Task GenerateDeck_ReturnsBadRequest_WhenServiceThrows()
        {
            _mockPrinterService.Setup(s => s.GenerateCardDeckPdfAsync(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<IProgress<int>>()))
                .ThrowsAsync(new Exception("No page was generated: all downloads failed."));

            var request = new PrintCardRequest { ImageUrls = new List<string> { "invalid" }, FileName = "Deck" };
            var result = await _controller.GenerateDeck(request);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("No page was generated: all downloads failed.", badRequest.Value);
        }

        [Fact]
        public async Task Generate_ReturnsFileResult_WithNullFileName()
        {
            var pdfBytes = new byte[] { 10, 20 };
            _mockPrinterService.Setup(s => s.GenerateCardPdfAsync(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<IProgress<int>>()))
                .ReturnsAsync(pdfBytes);

            var request = new PrintCardRequest { ImageUrls = new List<string> { "http://img.com/1.png" }, FileName = null! };
            var result = await _controller.Generate(request);

            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("PokemonCard.pdf", fileResult.FileDownloadName);
        }

        [Fact]
        public async Task GenerateDeck_ReturnsCorrectBytes()
        {
            var pdfBytes = new byte[] { 0xAA, 0xBB, 0xCC };
            _mockPrinterService.Setup(s => s.GenerateCardDeckPdfAsync(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<IProgress<int>>()))
                .ReturnsAsync(pdfBytes);

            var request = new PrintCardRequest { ImageUrls = new List<string> { "http://img.com/1.png", "http://img.com/2.png" }, FileName = "MultiCard" };
            var result = await _controller.GenerateDeck(request);

            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal(pdfBytes, fileResult.FileContents);
            Assert.Equal("MultiCard.pdf", fileResult.FileDownloadName);
        }
    }
}
