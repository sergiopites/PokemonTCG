using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class DownloadController : ControllerBase
{
    private readonly HttpClient _httpClient;

    public DownloadController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
    }

    [HttpGet("image")]
    public async Task<IActionResult> DownloadImage([FromQuery] string url, [FromQuery] string fileName = "image.png")
    {
        if (string.IsNullOrWhiteSpace(url))
            return BadRequest("URL is required");

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36");
            request.Headers.Referrer = new Uri("https://www.pokemontcg.io/");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode(); // aquí lanzará si es 404 u otro error

            var contentType = response.Content.Headers.ContentType?.ToString() ?? "image/png";
            var bytes = await response.Content.ReadAsByteArrayAsync();

            return File(bytes, contentType, fileName);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error downloading image: {ex.Message}");
        }
    }

}

