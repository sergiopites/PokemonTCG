using Microsoft.AspNetCore.Mvc;
using PokemonTCG.API.Request;
using PokemonTCG.API.Services;

namespace PokemonTCG.API.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class PrinterController : ControllerBase
    {
        private readonly IPrinterService _printerService;
        private readonly ILogger<PrinterController> _logger;

        public PrinterController(IPrinterService printerService, ILogger<PrinterController> logger)
        {
            _printerService = printerService;
            _logger = logger;
        }
        /// <summary>
        /// Genera un PDF imprimible de la carta y lo devuelve para descarga.
        /// </summary>
        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] PrintCardRequest request)
        {
            try
            {
                var pdfBytes = await _printerService.GenerateCardPdfAsync(request.ImageUrls, request.FileName);
                var outputFileName = string.IsNullOrWhiteSpace(request.FileName) ? "PokemonCard" : request.FileName;

                return File(pdfBytes, "application/pdf", outputFileName + ".pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating card PDF.");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("generatedeck")]
        public async Task<IActionResult> GenerateDeck([FromBody] PrintCardRequest request)
        {
            try
            {
                var pdfBytes = await _printerService.GenerateCardDeckPdfAsync(request.ImageUrls, request.FileName);
                var outputFileName = string.IsNullOrWhiteSpace(request.FileName) ? "PokemonCard" : request.FileName;

                return File(pdfBytes, "application/pdf", outputFileName + ".pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating deck PDF.");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("generate/progress")]
        public async Task GenerateWithProgress([FromBody] PrintCardRequest request)
        {
            Response.ContentType = "text/event-stream";
            Response.Headers.Append("Cache-Control", "no-cache");
            Response.Headers.Append("X-Accel-Buffering", "no");

            try
            {
                var lastReported = -1;
                var progress = new Progress<int>(async pct =>
                {
                    if (pct == lastReported) return;
                    lastReported = pct;
                    var data = $"data: {{\"progress\":{pct}}}\n\n";
                    await Response.WriteAsync(data);
                    await Response.Body.FlushAsync();
                });

                var pdfBytes = await _printerService.GenerateCardPdfAsync(request.ImageUrls, request.FileName, progress);
                var base64 = Convert.ToBase64String(pdfBytes);
                var outputFileName = string.IsNullOrWhiteSpace(request.FileName) ? "PokemonCard" : request.FileName;

                await Response.WriteAsync($"data: {{\"done\":true,\"fileName\":\"{outputFileName}.pdf\",\"pdf\":\"{base64}\"}}\n\n");
                await Response.Body.FlushAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating card PDF with progress.");
                await Response.WriteAsync($"data: {{\"error\":\"{ex.Message.Replace("\"", "'")}\"}}\n\n");
                await Response.Body.FlushAsync();
            }
        }

        [HttpPost("generatedeck/progress")]
        public async Task GenerateDeckWithProgress([FromBody] PrintCardRequest request)
        {
            Response.ContentType = "text/event-stream";
            Response.Headers.Append("Cache-Control", "no-cache");
            Response.Headers.Append("X-Accel-Buffering", "no");

            try
            {
                var lastReported = -1;
                var progress = new Progress<int>(async pct =>
                {
                    if (pct == lastReported) return;
                    lastReported = pct;
                    var data = $"data: {{\"progress\":{pct}}}\n\n";
                    await Response.WriteAsync(data);
                    await Response.Body.FlushAsync();
                });

                var pdfBytes = await _printerService.GenerateCardDeckPdfAsync(request.ImageUrls, request.FileName, progress);
                var base64 = Convert.ToBase64String(pdfBytes);
                var outputFileName = string.IsNullOrWhiteSpace(request.FileName) ? "PokemonCard" : request.FileName;

                await Response.WriteAsync($"data: {{\"done\":true,\"fileName\":\"{outputFileName}.pdf\",\"pdf\":\"{base64}\"}}\n\n");
                await Response.Body.FlushAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating deck PDF with progress.");
                await Response.WriteAsync($"data: {{\"error\":\"{ex.Message.Replace("\"", "'")}\"}}\n\n");
                await Response.Body.FlushAsync();
            }
        }

            [HttpPost("generateexcel/progress")]
            public async Task GenerateExcelWithProgress([FromBody] ExportExcelRequest request)
            {
                Response.ContentType = "text/event-stream";
                Response.Headers.Append("Cache-Control", "no-cache");
                Response.Headers.Append("X-Accel-Buffering", "no");

                try
                {
                    var lastReported = -1;
                    var progress = new Progress<int>(async pct =>
                    {
                        if (pct == lastReported) return;
                        lastReported = pct;
                        var data = $"data: {{\"progress\":{pct}}}\n\n";
                        await Response.WriteAsync(data);
                        await Response.Body.FlushAsync();
                    });

                    var excelBytes = await _printerService.GenerateCardsExcelAsync(request.Cards, request.FileName, progress);
                    var base64 = Convert.ToBase64String(excelBytes);
                    var outputFileName = string.IsNullOrWhiteSpace(request.FileName) ? "PokemonCards" : request.FileName;

                    await Response.WriteAsync($"data: {{\"done\":true,\"fileName\":\"{outputFileName}.xlsx\",\"excel\":\"{base64}\"}}\n\n");
                    await Response.Body.FlushAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error generating Excel with progress.");
                    await Response.WriteAsync($"data: {{\"error\":\"{ex.Message.Replace("\"", "'")}\"}}\n\n");
                    await Response.Body.FlushAsync();
                }
            }

        }
    }
