using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Drawing; // Importa el espacio de nombres System.Drawing
using System.Drawing.Imaging; // Importa el espacio de nombres System.Drawing.Imaging
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
                    _logger?.LogWarning("URL inválida: {Url}", url);
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

    }
}