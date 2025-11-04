namespace PokemonTCG.API.Services
{
    public interface IPrinterService
    {
        Task<byte[]> GenerateCardPdfAsync(List<string> imageUrls, string fileName);
        Task<byte[]> GenerateCardDeckPdfAsync(List<string> imageUrls, string fileName);
    }
}
