namespace PokemonTCG.API.Responses
{
    public class SetDetailResponse
    {
        public string SetId { get; set; }
        public string Name { get; set; }
        public string Series { get; set; }
        public long? PrintedTotal { get; set; }
        public long? Total { get; set; }
        public string PctgoCode { get; set; }
        public string ReleaseDate { get; set; }
        public string UpdatedAt { get; set; }
        public Uri Logo { get; set; }
        public Uri Symbol { get; set; }        
    }
}
