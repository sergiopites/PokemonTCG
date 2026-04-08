using PokemonTCG.Printer;

using ClosedXML.Excel;
using PokemonTCG.API.Request;

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
        public async Task<byte[]> GenerateCardPdfAsync(List<string> imageUrls, string fileName, IProgress<int> progress = null)
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

            return await _printer.SaveImagesToPdfAsync(validUrls, fileName, progress);
        }

        public async Task<byte[]> GenerateCardDeckPdfAsync(List<string> imageUrls, string fileName, IProgress<int> progress = null)
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

            return await _printer.SaveDeckImagesToPdfAsync(validUrls, fileName, progress);
        }

        public Task<byte[]> GenerateCardsExcelAsync(List<ExportCardItem> cards, string fileName, IProgress<int> progress = null)
        {
            if (cards == null || cards.Count == 0)
                throw new ArgumentException("Cards list cannot be empty.");

            if (string.IsNullOrWhiteSpace(fileName))
                fileName = "PokemonCards";

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Cards");

            // Header row
            var headers = new[] { "CardId", "Name", "Quantity", "Supertype", "Subtype", "Type", "HP", "Rarity", "Set", "SetId", "Number", "PtcgoCode", "Artist", "Evolves From", "Evolves To", "Image URL" };
            for (int col = 0; col < headers.Length; col++)
            {
                var cell = ws.Cell(1, col + 1);
                cell.Value = headers[col];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.DarkRed;
                cell.Style.Font.FontColor = XLColor.White;
            }

            int total = cards.Count;
            for (int i = 0; i < total; i++)
            {
                var c = cards[i];
                int row = i + 2;
                ws.Cell(row, 1).Value = c.CardId ?? "";
                ws.Cell(row, 2).Value = c.Name ?? "";
                ws.Cell(row, 3).Value = c.Quantity;
                ws.Cell(row, 4).Value = c.Supertype ?? "";
                ws.Cell(row, 5).Value = c.Subtype ?? "";
                ws.Cell(row, 6).Value = c.Type ?? "";
                ws.Cell(row, 7).Value = c.HP?.ToString() ?? "";
                ws.Cell(row, 8).Value = c.Rarity ?? "";
                ws.Cell(row, 9).Value = c.SetName ?? "";
                ws.Cell(row, 10).Value = c.SetId ?? "";
                ws.Cell(row, 11).Value = c.Number ?? "";
                ws.Cell(row, 12).Value = c.Ptcgocode ?? "";
                ws.Cell(row, 13).Value = c.Artist ?? "";
                ws.Cell(row, 14).Value = c.EvolvesFrom ?? "";
                ws.Cell(row, 15).Value = c.EvolvesTo ?? "";
                ws.Cell(row, 16).Value = c.ImageLarge ?? "";

                progress?.Report(total > 0 ? (int)((i + 1) * 100.0 / total) : 0);
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return Task.FromResult(stream.ToArray());
        }

    }
}
