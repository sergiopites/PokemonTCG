using Microsoft.AspNetCore.Mvc;
using PokemonTCG.API.Services;

namespace PokemonTCG.API.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class ScannerController : ControllerBase
    {
        private readonly IScannerService _scannerService;
        private readonly ICardService _cardService;
        private readonly ILogger<ScannerController> _logger;

        public ScannerController(IScannerService scannerService, ICardService cardService, ILogger<ScannerController> logger)
        {
            _scannerService = scannerService;
            _cardService = cardService;
            _logger = logger;
        }

        [HttpPost("scan")]
        public async Task<IActionResult> ScanCard([FromBody] ScanRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.ImageBase64))
                return BadRequest("No image provided.");

            try
            {
                var base64 = request.ImageBase64;
                var commaIdx = base64.IndexOf(',');
                if (commaIdx >= 0)
                    base64 = base64[(commaIdx + 1)..];

                var imageBytes = Convert.FromBase64String(base64);

                var scannedData = await _scannerService.ExtractCardDataFromImageAsync(imageBytes);

                if (string.IsNullOrWhiteSpace(scannedData.Name) && string.IsNullOrWhiteSpace(scannedData.Number))
                {
                    return Ok(new ScanResponse
                    {
                        ScannedData = scannedData,
                        Cards = null,
                        Message = "Could not read card data from the image. Try with better lighting and focus."
                    });
                }

                _logger.LogInformation(
                    "Scanning DB for Name=\"{Name}\", Supertype=\"{Supertype}\", Number=\"{Number}\"",
                    scannedData.Name, scannedData.Supertype, scannedData.Number);

                // Search by name first; if number was detected, include it for precision
                var results = await _cardService.SearchCardsAsync(
                    name: string.IsNullOrWhiteSpace(scannedData.Name) ? null : scannedData.Name,
                    supertype: string.IsNullOrWhiteSpace(scannedData.Supertype) ? null : scannedData.Supertype,
                    number: string.IsNullOrWhiteSpace(scannedData.Number) ? null : scannedData.Number,
                    page: 1,
                    pageSize: 20);

                // If no results by name+supertype+number, try name only
                if (!results.Items.Any() && !string.IsNullOrWhiteSpace(scannedData.Name))
                {
                    results = await _cardService.SearchCardsAsync(
                        name: scannedData.Name,
                        page: 1,
                        pageSize: 20);
                }

                return Ok(new ScanResponse
                {
                    ScannedData = scannedData,
                    Cards = results,
                    Message = results.Items.Any()
                        ? $"Found {results.TotalCount} card(s) matching the scanned data."
                        : $"No cards found for \"{scannedData.Name}\". Try scanning again with the card flat and well-lit."
                });
            }
            catch (FormatException)
            {
                return BadRequest("Invalid base64 image data.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scanning card");
                return StatusCode(500, "An error occurred while scanning the card.");
            }
        }
    }

    public class ScanRequest
    {
        public string ImageBase64 { get; set; } = string.Empty;
    }

    public class ScanResponse
    {
        public ScannedCardData? ScannedData { get; set; }
        public object? Cards { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
