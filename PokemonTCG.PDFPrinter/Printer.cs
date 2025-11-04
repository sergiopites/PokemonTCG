using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;

namespace PokemonTCG.Printer
{
    public class Printer
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<Printer> _logger;

        public Printer(ILogger<Printer> logger)
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; PokemonTCG-Printer/1.0)");
            _logger = logger;   
        }

        private const double CardWidth = 63 / 25.4 * 72;   // ≈ 178.58 puntos
        private const double CardHeight = 88 / 25.4 * 72;  // ≈ 249.45 puntos


        // DPI de impresión
        private const int Dpi = 300;

        /// <summary>
        /// Descarga una imagen desde un endpoint y la guarda en PDF en una ubicación elegida por el usuario.
        /// </summary>
        public async Task SaveImageFromEndpointAsync(string imageUrl, string fileName)
        {
            var configuration = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory()) // ruta base
              .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
              .Build();

            try
            {
                byte[] imageBytes = await _httpClient.GetByteArrayAsync(imageUrl);

                string filePdfPath = configuration["PdfPrinter:PkmTcgPdfPath"].ToString();
                //string filePngPath = configuration["PdfPrinter:PkmTcgPngPath"].ToString();

                SaveImageToPdf(imageBytes, filePdfPath, fileName + ".pdf");
                //DownloadCardAsync(fileName + ".png", filePngPath, imageUrl).Wait();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Inserta una imagen en un archivo PDF.
        /// </summary>
        private void SaveImageToPdf(byte[] imageBytes, string filePath, string fileName)
        {
            using var ms = new MemoryStream(imageBytes);
            using var img = XImage.FromStream(ms);
            var file = filePath + fileName;

            PdfDocument document = new PdfDocument();
            PdfPage page = document.AddPage();

            // ✅ Ajustar la página al tamaño real de la carta
            page.Width = CardWidth;
            page.Height = CardHeight;

            XGraphics gfx = XGraphics.FromPdfPage(page);

            // ✅ Dibujar la imagen escalada exactamente al tamaño de carta
            gfx.DrawImage(img, 0, 0, CardWidth, CardHeight);

            // Asegurar extensión .pdf
            //if (!filePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            //    filePath += ".pdf";
            if (!File.Exists(file))
            {
                document.Save(file);
            }
            else
                throw new Exception($"The file {file} already exists");
        }
        public async Task DownloadCardAsync(string fileName, string filePath, string imageUrl)
        {
            // Descargar los bytes de la imagen
            byte[] imageBytes = await _httpClient.GetByteArrayAsync(imageUrl);
            string file = filePath + fileName;
            // Asegurar que la carpeta exista
            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);

            if (!File.Exists(file))
            {
                // Guardar la imagen en disco
                await File.WriteAllBytesAsync(filePath + "//" + fileName, imageBytes);
            }
            else
            {
                throw new Exception($"The file {file} already exists");
            }
        }
        public async Task<byte[]> SaveImagesToPdfAsync(List<string> imageUrls, string fileName)
        {
            if (imageUrls == null || imageUrls.Count == 0)
                throw new ArgumentException("Image URLs cannot be empty.");

            using var document = new PdfDocument();

            foreach (var rawUrl in imageUrls)
            {
                var url = rawUrl?.Trim();
                _logger?.LogInformation("Intentando descargar: {Url}", url);

                if (!Uri.TryCreate(url, UriKind.Absolute, out var validatedUri))
                {
                    _logger?.LogWarning("Invalid Url: {Url}", url);
                    continue;
                }

                byte[] imageBytes;

                if (validatedUri.Scheme.ToLower() == "file")
                {
                    // Read from local file
                    try
                    {
                        imageBytes = await File.ReadAllBytesAsync(validatedUri.LocalPath);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Error reading local file: {FilePath}", validatedUri.LocalPath);
                        continue;
                    }
                }
                else
                {
                    // Download from web
                    var response = await _httpClient.GetAsync(validatedUri);
                    _logger?.LogInformation("Status {Status} para {Url}", response.StatusCode, url);
                    response.EnsureSuccessStatusCode();
                    imageBytes = await response.Content.ReadAsByteArrayAsync();
                }

                // Usar ImageSharp para re-encode a PNG
                using var ms = new MemoryStream(imageBytes);
                using var image = await Image.LoadAsync(ms);
                using var msConverted = new MemoryStream();
                await image.SaveAsync(msConverted, new PngEncoder());
                msConverted.Position = 0;

                using var img = XImage.FromStream(msConverted);

                var page = document.AddPage();
                page.Width = CardWidth;
                page.Height = CardHeight;

                using var gfx = XGraphics.FromPdfPage(page);
                gfx.DrawImage(img, 0, 0, CardWidth, CardHeight);
            }

            if (document.PageCount == 0)
                throw new Exception("No se generó ninguna página: todas las descargas fallaron.");

            using var stream = new MemoryStream();
            document.Save(stream, false);
            return stream.ToArray();
        }
        public async Task<byte[]> SaveDeckImagesToPdfAsync(List<string> imageUrls, string fileName)
        {
            if (imageUrls == null || imageUrls.Count == 0)
                throw new ArgumentException("Image URLs cannot be empty.");

            using var document = new PdfDocument();

            const double pageWidth = 595.28;   // A4 width en puntos (210mm)
            const double pageHeight = 841.89;  // A4 height en puntos (297mm)
            const int cols = 3;
            const int rows = 3;
            const double margin = 20;          // márgenes externos

            double availableWidth = pageWidth - margin * 2;
            double availableHeight = pageHeight - margin * 2;
            double cardWidth = availableWidth / cols;
            double cardHeight = availableHeight / rows;

            int index = 0;
            XGraphics gfx = null;
            PdfPage page = null;

            foreach (var rawUrl in imageUrls)
            {
                var url = rawUrl?.Trim();
                if (string.IsNullOrEmpty(url)) continue;

                _logger?.LogInformation("Intentando descargar: {Url}", url);

                if (!Uri.TryCreate(url, UriKind.Absolute, out var validatedUri))
                {
                    _logger?.LogWarning("Invalid Url: {Url}", url);
                    continue;
                }

                byte[] imageBytes;

                // Descargar imagen
                try
                {
                    if (validatedUri.Scheme.ToLower() == "file")
                    {
                        imageBytes = await File.ReadAllBytesAsync(validatedUri.LocalPath);
                    }
                    else
                    {
                        var response = await _httpClient.GetAsync(validatedUri);
                        response.EnsureSuccessStatusCode();
                        imageBytes = await response.Content.ReadAsByteArrayAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error descargando imagen {Url}", url);
                    continue;
                }

                // Convertir a PNG
                using var ms = new MemoryStream(imageBytes);
                using var image = await Image.LoadAsync(ms);
                using var msConverted = new MemoryStream();
                await image.SaveAsync(msConverted, new PngEncoder());
                msConverted.Position = 0;

                using var img = XImage.FromStream(msConverted);

                // Crear nueva página si es necesario
                if (index % (cols * rows) == 0)
                {
                    page = document.AddPage();
                    page.Width = pageWidth;
                    page.Height = pageHeight;
                    gfx = XGraphics.FromPdfPage(page);
                }

                int posInPage = index % (cols * rows);
                int col = posInPage % cols;
                int row = posInPage / cols;

                double x = margin + col * cardWidth;
                double y = margin + row * cardHeight;

                gfx.DrawImage(img, x, y, cardWidth, cardHeight);
                index++;
            }

            if (document.PageCount == 0)
                throw new Exception("No se generó ninguna página: todas las descargas fallaron.");

            using var stream = new MemoryStream();
            document.Save(stream, false);
            return stream.ToArray();
        }

        //public async Task<byte[]> SaveDeckImagesToPdfAsync(List<string> imageUrls, string fileName)
        //{
        //    if (imageUrls == null || imageUrls.Count == 0)
        //        throw new ArgumentException("Image URLs cannot be empty.");

        //    // 🧮 Medidas oficiales Pokémon
        //    const double mmToPt = 2.8346;
        //    const double cardWidth = 63 * mmToPt;   // 179 pt
        //    const double cardHeight = 88 * mmToPt;  // 249 pt
        //    const double spacing = 2 * mmToPt;      // 2 mm entre cartas

        //    const int cardsPerRow = 10;
        //    const int cardsPerColumn = 11;

        //    double sheetWidth = (cardWidth * cardsPerRow) + (spacing * (cardsPerRow - 1));
        //    double sheetHeight = (cardHeight * cardsPerColumn) + (spacing * (cardsPerColumn - 1));

        //    using var document = new PdfDocument();
        //    int totalCards = imageUrls.Count;
        //    int cardsPerSheet = cardsPerRow * cardsPerColumn;

        //    for (int i = 0; i < totalCards; i += cardsPerSheet)
        //    {
        //        var page = document.AddPage();
        //        page.Width = sheetWidth;
        //        page.Height = sheetHeight;

        //        using var gfx = XGraphics.FromPdfPage(page);

        //        var batch = imageUrls.Skip(i).Take(cardsPerSheet).ToList();
        //        int index = 0;

        //        foreach (var rawUrl in batch)
        //        {
        //            var url = rawUrl?.Trim();
        //            if (string.IsNullOrEmpty(url))
        //                continue;

        //            byte[] imageBytes;

        //            if (Uri.TryCreate(url, UriKind.Absolute, out var validatedUri))
        //            {
        //                if (validatedUri.Scheme.ToLower() == "file")
        //                {
        //                    imageBytes = await File.ReadAllBytesAsync(validatedUri.LocalPath);
        //                }
        //                else
        //                {
        //                    var response = await _httpClient.GetAsync(validatedUri);
        //                    response.EnsureSuccessStatusCode();
        //                    imageBytes = await response.Content.ReadAsByteArrayAsync();
        //                }
        //            }
        //            else
        //            {
        //                continue;
        //            }

        //            using var ms = new MemoryStream(imageBytes);
        //            using var image = await Image.LoadAsync(ms);
        //            using var msConverted = new MemoryStream();
        //            await image.SaveAsync(msConverted, new PngEncoder());
        //            msConverted.Position = 0;

        //            using var img = XImage.FromStream(msConverted);

        //            int row = index / cardsPerRow;
        //            int col = index % cardsPerRow;

        //            double x = col * (cardWidth + spacing);
        //            double y = row * (cardHeight + spacing);

        //            gfx.DrawImage(img, x, y, cardWidth, cardHeight);
        //            index++;
        //        }
        //    }

        //    if (document.PageCount == 0)
        //        throw new Exception("No se generó ninguna plancha: todas las descargas fallaron.");

        //    using var stream = new MemoryStream();
        //    document.Save(stream, false);
        //    return stream.ToArray();
        //}


    }
}