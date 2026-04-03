using Microsoft.AspNetCore.Mvc;
using Moq;
using PokemonTCG.API.Controllers;
using PokemonTCG.API.Request;
using PokemonTCG.API.Services;

namespace PokemonTCG.Test.Controllers
{
    public class PrinterControllerTests
    {
        private readonly Mock<IPrinterService> _mockPrinterService;
        private readonly PrinterController _controller;

        public PrinterControllerTests()
        {
            _mockPrinterService = new Mock<IPrinterService>();
            _controller = new PrinterController(_mockPrinterService.Object);
        }

        [Fact]
        public async Task Generate_ReturnsFileResult()
        {
            var pdfBytes = new byte[] { 1, 2, 3 };
            _mockPrinterService.Setup(s => s.GenerateCardPdfAsync(It.IsAny<List<string>>(), "TestCard"))
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
            _mockPrinterService.Setup(s => s.GenerateCardPdfAsync(It.IsAny<List<string>>(), It.IsAny<string>()))
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
            _mockPrinterService.Setup(s => s.GenerateCardDeckPdfAsync(It.IsAny<List<string>>(), "MyDeck"))
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
            _mockPrinterService.Setup(s => s.GenerateCardDeckPdfAsync(It.IsAny<List<string>>(), It.IsAny<string>()))
                .ReturnsAsync(pdfBytes);

            var request = new PrintCardRequest { ImageUrls = new List<string> { "http://img.com/1.png" }, FileName = "" };
            var result = await _controller.GenerateDeck(request);

            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("PokemonCard.pdf", fileResult.FileDownloadName);
        }
    }
}
