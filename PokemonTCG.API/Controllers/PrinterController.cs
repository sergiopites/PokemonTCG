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

        public PrinterController(IPrinterService printerService)
        {
            _printerService = printerService;
        }

        /// <summary>
        /// Genera un PDF imprimible de la carta y lo devuelve para descarga.
        /// </summary>
        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] PrintCardRequest request)
        {
            var pdfBytes = await _printerService.GenerateCardPdfAsync(request.ImageUrls, request.FileName);
            var outputFileName = string.IsNullOrWhiteSpace(request.FileName) ? "PokemonCard" : request.FileName;

            return File(pdfBytes, "application/pdf", outputFileName + ".pdf");
        }


    }
}
