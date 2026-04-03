using Microsoft.AspNetCore.Mvc;
using Moq;
using PokemonTCG.API.Controllers;
using PokemonTCG.API.Models;
using PokemonTCG.API.Services;

namespace PokemonTCG.Test.Controllers
{
    public class AttackControllerTests
    {
        private readonly Mock<IAttackService> _mockAttackService;
        private readonly AttackController _controller;

        public AttackControllerTests()
        {
            _mockAttackService = new Mock<IAttackService>();
            _controller = new AttackController(_mockAttackService.Object);
        }

        [Fact]
        public async Task GetAttacksByCardId_ReturnsOk_WhenAttacksExist()
        {
            var attacks = new List<Attack> { new() { Name = "Thunder Shock", Damage = "30" } };
            _mockAttackService.Setup(s => s.GetAttacksByCardIdAsync("xy1-1")).ReturnsAsync(attacks);

            var result = await _controller.GetAttacksByCardId("xy1-1");

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetAttacksByCardId_ReturnsNotFound_WhenEmpty()
        {
            _mockAttackService.Setup(s => s.GetAttacksByCardIdAsync("nope")).ReturnsAsync(new List<Attack>());

            var result = await _controller.GetAttacksByCardId("nope");

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetAttacksByCardId_ReturnsNotFound_WhenNull()
        {
            _mockAttackService.Setup(s => s.GetAttacksByCardIdAsync("nope")).ReturnsAsync((List<Attack>)null!);

            var result = await _controller.GetAttacksByCardId("nope");

            Assert.IsType<NotFoundResult>(result.Result);
        }
    }
}
