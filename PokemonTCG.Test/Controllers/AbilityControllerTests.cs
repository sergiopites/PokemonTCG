using Microsoft.AspNetCore.Mvc;
using Moq;
using PokemonTCG.API.Controllers;
using PokemonTCG.API.Models;
using PokemonTCG.API.Services;

namespace PokemonTCG.Test.Controllers
{
    public class AbilityControllerTests
    {
        private readonly Mock<IAbilityService> _mockAbilityService;
        private readonly AbilityController _controller;

        public AbilityControllerTests()
        {
            _mockAbilityService = new Mock<IAbilityService>();
            _controller = new AbilityController(_mockAbilityService.Object);
        }

        [Fact]
        public async Task GetAbilitiesByCardId_ReturnsOk_WhenAbilitiesExist()
        {
            var abilities = new List<Ability>
            {
                new() { AbilityId = 1, Name = "Static", Text = "Paralyze", Type = "Ability", CardId = "xy1-1" }
            };
            _mockAbilityService.Setup(s => s.GetAbilitiesByCardIdAsync("xy1-1")).ReturnsAsync(abilities);

            var result = await _controller.GetAbilitiesByCardId("xy1-1");

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returned = Assert.IsType<List<Ability>>(okResult.Value);
            Assert.Single(returned);
        }

        [Fact]
        public async Task GetAbilitiesByCardId_ReturnsNotFound_WhenEmpty()
        {
            _mockAbilityService.Setup(s => s.GetAbilitiesByCardIdAsync("xy1-999")).ReturnsAsync(new List<Ability>());

            var result = await _controller.GetAbilitiesByCardId("xy1-999");

            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("No abilities found", notFound.Value);
        }

        [Fact]
        public async Task GetAbilitiesByCardId_ReturnsNotFound_WhenNull()
        {
            _mockAbilityService.Setup(s => s.GetAbilitiesByCardIdAsync("xy1-999")).ReturnsAsync((List<Ability>)null!);

            var result = await _controller.GetAbilitiesByCardId("xy1-999");

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }
    }
}
