namespace PokemonTCG.API.Request
{
    public class ExportExcelRequest
    {
        public List<ExportCardItem> Cards { get; set; } = new();
        public string FileName { get; set; } = string.Empty;
    }

    public class ExportCardItem
    {
        public string CardId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Supertype { get; set; } = string.Empty;
        public string Subtype { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Rarity { get; set; } = string.Empty;
        public string SetName { get; set; } = string.Empty;
        public string SetId { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string Ptcgocode { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;
        public string ImageLarge { get; set; } = string.Empty;
        public int? HP { get; set; }
        public string EvolvesFrom { get; set; } = string.Empty;
        public string EvolvesTo { get; set; } = string.Empty;
    }
}
