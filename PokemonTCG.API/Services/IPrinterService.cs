namespace PokemonTCG.API.Services
{
    public interface IPrinterService
    {
        Task<byte[]> GenerateCardPdfAsync(List<string> imageUrls, string fileName, IProgress<int> progress = null);
        Task<byte[]> GenerateCardDeckPdfAsync(List<string> imageUrls, string fileName, IProgress<int> progress = null);
        Task<byte[]> GenerateCardsExcelAsync(List<Request.ExportCardItem> cards, string fileName, IProgress<int> progress = null);
    }
}
