using PokemonTCG.SDK.Infrastructure.HttpClients.Set;

namespace PokemonTCG.Test.SDK
{
    public class PokemonSetApiResourceTests
    {
        [Fact]
        public void PokemonSetApiResource_DefaultValues_AreNull()
        {
            var resource = new PokemonSetApiResource();

            Assert.Null(resource.Id);
            Assert.Null(resource.Name);
            Assert.Null(resource.Series);
            Assert.Null(resource.PrintedTotal);
            Assert.Null(resource.Total);
            Assert.Null(resource.PtcgoCode);
            Assert.Null(resource.ReleaseDate);
            Assert.Null(resource.UpdatedAt);
            Assert.Null(resource.Legalities);
            Assert.Null(resource.Images);
        }

        [Fact]
        public void PokemonSetApiResource_SetProperties_ReturnCorrectValues()
        {
            var legalities = new SetLegalityApiResource
            {
                Expanded = "Legal",
                Standard = "Legal",
                Unlimited = "Legal"
            };
            var images = new SetImagesApiResource
            {
                Logo = "https://logo.png",
                Symbol = "https://symbol.png"
            };

            var resource = new PokemonSetApiResource
            {
                Id = "base1",
                Name = "Base Set",
                Series = "Base",
                PrintedTotal = 102,
                Total = 102,
                PtcgoCode = "BS",
                ReleaseDate = "1999/01/09",
                UpdatedAt = "2020/08/14",
                Legalities = legalities,
                Images = images
            };

            Assert.Equal("base1", resource.Id);
            Assert.Equal("Base Set", resource.Name);
            Assert.Equal("Base", resource.Series);
            Assert.Equal(102, resource.PrintedTotal);
            Assert.Equal(102, resource.Total);
            Assert.Equal("BS", resource.PtcgoCode);
            Assert.Equal("1999/01/09", resource.ReleaseDate);
            Assert.Equal("2020/08/14", resource.UpdatedAt);
            Assert.Same(legalities, resource.Legalities);
            Assert.Same(images, resource.Images);
        }

        [Fact]
        public void SetLegalityApiResource_SetProperties_ReturnCorrectValues()
        {
            var legality = new SetLegalityApiResource
            {
                Expanded = "Legal",
                Standard = "Banned",
                Unlimited = "Legal"
            };

            Assert.Equal("Legal", legality.Expanded);
            Assert.Equal("Banned", legality.Standard);
            Assert.Equal("Legal", legality.Unlimited);
        }

        [Fact]
        public void SetLegalityApiResource_DefaultValues_AreNull()
        {
            var legality = new SetLegalityApiResource();

            Assert.Null(legality.Expanded);
            Assert.Null(legality.Standard);
            Assert.Null(legality.Unlimited);
        }

        [Fact]
        public void SetImagesApiResource_SetProperties_ReturnCorrectValues()
        {
            var images = new SetImagesApiResource
            {
                Logo = "https://logo.png",
                Symbol = "https://symbol.png"
            };

            Assert.Equal("https://logo.png", images.Logo);
            Assert.Equal("https://symbol.png", images.Symbol);
        }

        [Fact]
        public void SetImagesApiResource_DefaultValues_AreNull()
        {
            var images = new SetImagesApiResource();

            Assert.Null(images.Logo);
            Assert.Null(images.Symbol);
        }
    }
}
