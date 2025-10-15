using PokemonTCG.Printer;

namespace PokemonTCG.API.Services
{
    public class PrinterService: IPrinterService
    {
        private readonly PokemonTCG.Printer.Printer _printer;        
        public PrinterService(ILogger<Printer.Printer> logger)
        {
            _printer = new PokemonTCG.Printer.Printer(logger);            
        }

        /// <summary>
        /// Genera y guarda un PDF imprimible de una carta Pokémon.
        /// </summary>
        // PrinterService.cs
        public async Task<byte[]> GenerateCardPdfAsync(List<string> imageUrls, string fileName)
        {
            if (imageUrls == null || imageUrls.Count == 0)
                throw new ArgumentException("Image URLs cannot be empty.");

            if (string.IsNullOrWhiteSpace(fileName))
                fileName = "PokemonCard";

            List<string> validUrls = new List<string>();

            foreach (var url in imageUrls)
            {
                //if (Uri.IsWellFormedUriString(url, UriKind.Absolute) && Uri.TryCreate(url, UriKind.Absolute, out _))
                //{
                    validUrls.Add(url);
                //}
            }

            return await _printer.SaveImagesToPdfAsync(validUrls, fileName);
        }


    }
}
