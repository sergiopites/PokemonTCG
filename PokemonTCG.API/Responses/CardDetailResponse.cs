namespace PokemonTCG.API.Responses
{
    public class CardDetailResponse
    {
        public string CardId { get; set; }
        public Uri ImageLarge { get; set; }
        public string Name { get; set; }
        public string SetName { get; set; }
        public string SetId { get; set; }
        public string Rule { get; set; }
        public string Number { get; set; }
        public string Subtype { get; set; }
        public string Type { get; set; }
        public string Supertype { get; set; }
        public List<AttackDetailResponse> AttackDetails { get; set; }
        public List<AbilityDetailResponse> AbilityDetails { get; set; }
        public List<ResistanceDetailResponse> ResistanceDetails { get; set; }
        public List<WeaknessDetailResponse> WeaknessDetails { get; set; }
        public Uri CardMarketUrl { get; set; }
        public Uri TcgPlayerUrl { get; set; }
        public string Rarity { get; set; }
        public Uri SetImage { get; set; }
        public Uri SetSymbol { get; set; }
        public string Artist { get; set; }
        public string EvolvesFrom { get; set; }
        public string EvolvesTo { get; set; }
        public int? HP { get; set; }
        public string SetPrintedTotal { get; set; }
        public string SetTotal { get; set; }
        public int? ConvertTreatCost { get; set; }
        public string RetreatCost { get; set; }
        public string SetSerie { get; set; }
        public string Ptcgocode { get; set; }
        public string ReleaseDate { get; set; }
    }
}
