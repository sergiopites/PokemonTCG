using Microsoft.Extensions.Logging;
using Moq;
using PokemonTCG.API.Models;
using PokemonTCG.API.Repositories;
using PokemonTCG.API.Services;

namespace PokemonTCG.Test.Services
{
    public class CardImageServiceTests
    {
        private readonly Mock<ICardImageRepository> _repo = new();
        private readonly Mock<ILogger<CardImageService>> _logger = new();

        private CardImageService CreateService() => new(_repo.Object, _logger.Object);

        // ?? SaveImageCard ????????????????????????????????????????????????

        [Fact]
        public void SaveImageCard_ValidImage_CallsRepository()
        {
            var image = new CardImage { Small = new Uri("https://small.png"), Large = new Uri("https://large.png") };

            CreateService().SaveImageCard(image, CancellationToken.None);

            _repo.Verify(r => r.SaveImageCardAsync(image, It.IsAny<CancellationToken>()), Times.Once);
        }

        // ?? GetAllImagesAsync ????????????????????????????????????????????

        [Fact]
        public async Task GetAllImagesAsync_ReturnsData()
        {
            var expected = new List<CardImage>
            {
                new() { CardImageId = 1, Small = new Uri("https://a.png"), Large = new Uri("https://A.png") }
            };
            _repo.Setup(r => r.GetAllImagesAsync()).ReturnsAsync(expected);

            var result = await CreateService().GetAllImagesAsync();

            Assert.Single(result);
        }

        [Fact]
        public async Task GetAllImagesAsync_RepoThrows_ReturnsEmpty()
        {
            _repo.Setup(r => r.GetAllImagesAsync()).ThrowsAsync(new Exception("DB error"));

            var result = await CreateService().GetAllImagesAsync();

            Assert.Empty(result);
        }
    }
}
