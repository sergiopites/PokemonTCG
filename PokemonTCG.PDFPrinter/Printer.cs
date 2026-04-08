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
        private const int Dpi = 300;        
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
        public async Task<byte[]> SaveImagesToPdfAsync(List<string> imageUrls, string fileName, IProgress<int> progress = null)
        {
            if (imageUrls == null || imageUrls.Count == 0)
                throw new ArgumentException("Image URLs cannot be empty.");

            using var document = new PdfDocument();
            int processed = 0;
            int total = imageUrls.Count;

            foreach (var rawUrl in imageUrls)
            {
                var url = rawUrl?.Trim();
                _logger?.LogInformation("Trying to download: {Url}", url);

                if (!Uri.TryCreate(url, UriKind.Absolute, out var validatedUri))
                {
                    _logger?.LogWarning("Invalid Url: {Url}", url);
                    continue;
                }

                byte[] imageBytes;

                if (validatedUri.Scheme.ToLower() == "file")
                {                    
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
                    var response = await _httpClient.GetAsync(validatedUri);
                    _logger?.LogInformation("Status {Status} for {Url}", response.StatusCode, url);
                    response.EnsureSuccessStatusCode();
                    imageBytes = await response.Content.ReadAsByteArrayAsync();
                }
                
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

                processed++;
                progress?.Report(total > 0 ? (int)(processed * 100.0 / total) : 0);
            }

            if (document.PageCount == 0)
                throw new Exception("No page was generated: all downloads failed.");

            using var stream = new MemoryStream();
            document.Save(stream, false);
            return stream.ToArray();
        }
        public async Task<byte[]> SaveDeckImagesToPdfAsync(List<string> imageUrls, string fileName, IProgress<int> progress = null)
        {
            if (imageUrls == null || imageUrls.Count == 0)
                throw new ArgumentException("Image URLs cannot be empty.");

            using var document = new PdfDocument();

            const double pageWidth = 595.28;
            const double pageHeight = 841.89;
            const int cols = 3;
            const int rows = 3;
            const double margin = 20;

            double availableWidth = pageWidth - margin * 2;
            double availableHeight = pageHeight - margin * 2;
            double cardWidth = availableWidth / cols;
            double cardHeight = availableHeight / rows;

            int index = 0;
            int processed = 0;
            int total = imageUrls.Count;
            XGraphics gfx = null;
            PdfPage page = null;

            foreach (var rawUrl in imageUrls)
            {
                var url = rawUrl?.Trim();
                if (string.IsNullOrEmpty(url)) { processed++; continue; }

                _logger?.LogInformation("Trying to download: {Url}", url);

                if (!Uri.TryCreate(url, UriKind.Absolute, out var validatedUri))
                {
                    _logger?.LogWarning("Invalid Url: {Url}", url);
                    continue;
                }

                byte[] imageBytes;

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
                    _logger?.LogError(ex, "Error downloading image {Url}", url);
                    continue;
                }

                using var ms = new MemoryStream(imageBytes);
                using var image = await Image.LoadAsync(ms);
                using var msConverted = new MemoryStream();
                await image.SaveAsync(msConverted, new PngEncoder());
                msConverted.Position = 0;

                using var img = XImage.FromStream(msConverted);

                if (index % (cols * rows) == 0)
                {
                    gfx?.Dispose();
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
                processed++;
                progress?.Report(total > 0 ? (int)(processed * 100.0 / total) : 0);
            }

            gfx?.Dispose();

            if (document.PageCount == 0)
                throw new Exception("No page was generated: all downloads failed.");

            using var stream = new MemoryStream();
            document.Save(stream, false);
            return stream.ToArray();
        }   
    }
}