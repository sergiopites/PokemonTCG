using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Net;
using System.Net.Http;
using System.Threading;

namespace PokemonTCG.Test.Controllers
{
    public class DownloadControllerTests
    {
        private static DownloadController CreateController(HttpMessageHandler handler)
        {
            var factory = new Mock<IHttpClientFactory>();
            factory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(new HttpClient(handler));
            return new DownloadController(factory.Object);
        }

        [Fact]
        public async Task DownloadImage_ReturnsBadRequest_WhenUrlIsNull()
        {
            var controller = CreateController(new FakeHandler(HttpStatusCode.OK, new byte[0]));
            var result = await controller.DownloadImage(null!);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task DownloadImage_ReturnsBadRequest_WhenUrlIsEmpty()
        {
            var controller = CreateController(new FakeHandler(HttpStatusCode.OK, new byte[0]));
            var result = await controller.DownloadImage("");
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task DownloadImage_ReturnsBadRequest_WhenUrlIsWhitespace()
        {
            var controller = CreateController(new FakeHandler(HttpStatusCode.OK, new byte[0]));
            var result = await controller.DownloadImage("   ");
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task DownloadImage_ReturnsFile_WhenSuccessful()
        {
            var imageBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47 }; // PNG header
            var controller = CreateController(new FakeHandler(HttpStatusCode.OK, imageBytes, "image/png"));

            var result = await controller.DownloadImage("https://example.com/image.png", "test.png");

            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("image/png", fileResult.ContentType);
            Assert.Equal("test.png", fileResult.FileDownloadName);
            Assert.Equal(imageBytes, fileResult.FileContents);
        }

        [Fact]
        public async Task DownloadImage_UsesDefaultFileName_WhenNotProvided()
        {
            var controller = CreateController(new FakeHandler(HttpStatusCode.OK, new byte[] { 1, 2 }, "image/png"));

            var result = await controller.DownloadImage("https://example.com/image.png");

            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("image.png", fileResult.FileDownloadName);
        }

        [Fact]
        public async Task DownloadImage_ReturnsBadRequest_WhenHttpRequestFails()
        {
            var controller = CreateController(new FakeHandler(HttpStatusCode.NotFound, null));

            var result = await controller.DownloadImage("https://example.com/missing.png");

            Assert.IsType<BadRequestObjectResult>(result);
        }

        /// <summary>Minimal HttpMessageHandler stub for tests.</summary>
        private class FakeHandler : HttpMessageHandler
        {
            private readonly HttpStatusCode _statusCode;
            private readonly byte[]? _content;
            private readonly string _contentType;

            public FakeHandler(HttpStatusCode statusCode, byte[]? content, string contentType = "image/png")
            {
                _statusCode = statusCode;
                _content = content;
                _contentType = contentType;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var response = new HttpResponseMessage(_statusCode);
                if (_content != null)
                {
                    response.Content = new ByteArrayContent(_content);
                    response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(_contentType);
                }
                return Task.FromResult(response);
            }
        }
    }
}
