namespace PokemonTCG.API.Request
{
    public class PrintCardRequest
    {
        public List<string> ImageUrls { get; set; } = new();
        public string FileName { get; set; } = string.Empty;
    }
}
