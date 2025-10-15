namespace PokemonTCG.API.Services
{
    public interface IPrinterService
    {
        Task<byte[]> GenerateCardPdfAsync(List<string> imageUrls, string fileName);
    }
}
