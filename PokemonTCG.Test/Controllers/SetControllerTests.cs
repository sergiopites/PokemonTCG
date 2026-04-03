using Microsoft.AspNetCore.Mvc;
using Moq;
using PokemonTCG.API.Controllers;
using PokemonTCG.API.Responses;
using PokemonTCG.API.Services;

namespace PokemonTCG.Test.Controllers
{
    public class SetControllerTests
    {
        private readonly Mock<ISetService> _mockSetService;
        private readonly SetController _controller;

        public SetControllerTests()
        {
            _mockSetService = new Mock<ISetService>();
            _controller = new SetController(_mockSetService.Object);
        }

        // SaveSet
        [Fact]
        public async Task SaveSet_ReturnsOk()
        {
            _mockSetService.Setup(s => s.SaveSetAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await _controller.SaveSet(CancellationToken.None);

            Assert.IsType<OkResult>(result);
        }

        // GetAllSets
        [Fact]
        public async Task GetAllSets_ReturnsOk_WhenSetsExist()
        {
            var sets = new List<SetDetailResponse> { new() { SetId = "xy1", Name = "XY" } };
            _mockSetService.Setup(s => s.GetAllSetsAsync()).ReturnsAsync(sets);

            var result = await _controller.GetAllSets();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returned = Assert.IsType<List<SetDetailResponse>>(okResult.Value);
            Assert.Single(returned);
        }

        [Fact]
        public async Task GetAllSets_ReturnsNotFound_WhenEmpty()
        {
            _mockSetService.Setup(s => s.GetAllSetsAsync()).ReturnsAsync(new List<SetDetailResponse>());

            var result = await _controller.GetAllSets();

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetAllSets_ReturnsNotFound_WhenNull()
        {
            _mockSetService.Setup(s => s.GetAllSetsAsync()).ReturnsAsync((List<SetDetailResponse>)null!);

            var result = await _controller.GetAllSets();

            Assert.IsType<NotFoundResult>(result.Result);
        }

        // GetSetByName
        [Fact]
        public async Task GetSetByName_ReturnsOk_WhenFound()
        {
            var sets = new List<SetDetailResponse> { new() { SetId = "xy1", Name = "XY" } };
            _mockSetService.Setup(s => s.GetSetByNameAsync("XY")).ReturnsAsync(sets);

            var result = await _controller.GetSetByName("XY");

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetSetByName_ReturnsNotFound_WhenEmpty()
        {
            _mockSetService.Setup(s => s.GetSetByNameAsync("missing")).ReturnsAsync(new List<SetDetailResponse>());

            var result = await _controller.GetSetByName("missing");

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetSetByName_ReturnsNotFound_WhenNull()
        {
            _mockSetService.Setup(s => s.GetSetByNameAsync("missing")).ReturnsAsync((List<SetDetailResponse>)null!);

            var result = await _controller.GetSetByName("missing");

            Assert.IsType<NotFoundResult>(result.Result);
        }

        // GetSetById
        [Fact]
        public async Task GetSetById_ReturnsOk_WhenFound()
        {
            var sets = new List<SetDetailResponse> { new() { SetId = "xy1" } };
            _mockSetService.Setup(s => s.GetSetByIdAsync("xy1")).ReturnsAsync(sets);

            var result = await _controller.GetSetById("xy1");

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetSetById_ReturnsNotFound_WhenEmpty()
        {
            _mockSetService.Setup(s => s.GetSetByIdAsync("nope")).ReturnsAsync(new List<SetDetailResponse>());

            var result = await _controller.GetSetById("nope");

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetSetById_ReturnsNotFound_WhenNull()
        {
            _mockSetService.Setup(s => s.GetSetByIdAsync("nope")).ReturnsAsync((List<SetDetailResponse>)null!);

            var result = await _controller.GetSetById("nope");

            Assert.IsType<NotFoundResult>(result.Result);
        }

        // GetSetBySerie
        [Fact]
        public async Task GetSetBySerie_ReturnsOk_WhenFound()
        {
            var sets = new List<SetDetailResponse> { new() { SetId = "xy1", Series = "XY" } };
            _mockSetService.Setup(s => s.GetSetBySerieAsync("XY")).ReturnsAsync(sets);

            var result = await _controller.GetSetBySerie("XY");

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetSetBySerie_ReturnsNotFound_WhenEmpty()
        {
            _mockSetService.Setup(s => s.GetSetBySerieAsync("none")).ReturnsAsync(new List<SetDetailResponse>());

            var result = await _controller.GetSetBySerie("none");

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetSetBySerie_ReturnsNotFound_WhenNull()
        {
            _mockSetService.Setup(s => s.GetSetBySerieAsync("none")).ReturnsAsync((List<SetDetailResponse>)null!);

            var result = await _controller.GetSetBySerie("none");

            Assert.IsType<NotFoundResult>(result.Result);
        }
    }
}
