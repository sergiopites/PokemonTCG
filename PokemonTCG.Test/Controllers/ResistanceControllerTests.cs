using Microsoft.AspNetCore.Mvc;
using Moq;
using PokemonTCG.API.Controllers;
using PokemonTCG.API.Models;
using PokemonTCG.API.Services;

namespace PokemonTCG.Test.Controllers
{
    public class ResistanceControllerTests
    {
        private readonly Mock<IResistanceService> _mockResistanceService;
        private readonly ResistanceController _controller;

        public ResistanceControllerTests()
        {
            _mockResistanceService = new Mock<IResistanceService>();
            _controller = new ResistanceController(_mockResistanceService.Object);
        }

        [Fact]
        public async Task GetAbilitiesByCardId_ReturnsOk_WhenResistancesExist()
        {
            var resistances = new List<Resistance>
            {
                new() { ResistanceId = 1, Type = "Metal", Value = "-20", CardId = "xy1-1" }
            };
            _mockResistanceService.Setup(s => s.GetResistanceByCardId("xy1-1")).ReturnsAsync(resistances);

            var result = await _controller.GetAbilitiesByCardId("xy1-1");

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returned = Assert.IsType<List<Resistance>>(okResult.Value);
            Assert.Single(returned);
        }

        [Fact]
        public async Task GetAbilitiesByCardId_ReturnsNotFound_WhenEmpty()
        {
            _mockResistanceService.Setup(s => s.GetResistanceByCardId("nope")).ReturnsAsync(new List<Resistance>());

            var result = await _controller.GetAbilitiesByCardId("nope");

            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("No resistances found", notFound.Value);
        }

        [Fact]
        public async Task GetAbilitiesByCardId_ReturnsNotFound_WhenNull()
        {
            _mockResistanceService.Setup(s => s.GetResistanceByCardId("nope")).ReturnsAsync((List<Resistance>)null!);

            var result = await _controller.GetAbilitiesByCardId("nope");

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }
    }
}
