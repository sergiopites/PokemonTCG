using PokemonTCG.SDK.Infrastructure.HttpClients;

namespace PokemonTCG.SDK.Infrastructure.HttpClients.Set
{
    public class PokemonSetApiResource : ApiResource
    {
        private static string ApiEndpoint { get; } = "sets";
        
        public override string Id { get; set; }
        public string Name { get; set; }
        public string Series { get; set; }
        public long? PrintedTotal { get; set; }
        public long? Total { get; set; }
        public string PtcgoCode { get; set; }
        public string ReleaseDate { get; set; }
        public string UpdatedAt { get; set; }
        public SetLegalityApiResource Legalities { get; set; }
        public SetImagesApiResource Images { get; set; }
    }
    
    public class SetLegalityApiResource
    {
        public string Expanded { get; set; }
        public string Standard { get; set; }
        public string Unlimited { get; set; }
    }
    
    public class SetImagesApiResource
    {
        public string Logo { get; set; }
        public string Symbol { get; set; }
    }
}