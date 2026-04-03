namespace PokemonTCG.API.Services
{
    public interface IScannerService
    {
        Task<ScannedCardData> ExtractCardDataFromImageAsync(byte[] imageBytes);
    }

    public class ScannedCardData
    {
        public string Name { get; set; } = string.Empty;
        public string HP { get; set; } = string.Empty;
        public string Supertype { get; set; } = string.Empty;
        public string Stage { get; set; } = string.Empty;
        public string EvolvesFrom { get; set; } = string.Empty;
        public List<string> Attacks { get; set; } = new();
        public string Weakness { get; set; } = string.Empty;
        public string Resistance { get; set; } = string.Empty;
        public string RetreatCost { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public string FullText { get; set; } = string.Empty;
    }
}
