namespace PokemonTCG.API.Request
{
    public class CollectionRequest
    {
        public int? CollectionId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ICollection<CollectionCardQuantityRequest> Cards { get; set; } = new List<CollectionCardQuantityRequest>();
    }

    public class CollectionCardQuantityRequest
    {
        public string CardId { get; set; }
        public int Quantity { get; set; }
        public string? Name { get; set; }
        public string? ImageLarge { get; set; }
        public string? Supertype { get; set; }
        public string? Subtype { get; set; }
        public string? Number { get; set; }
        public string? SetName { get; set; }
        public string? SetId { get; set; }
        public string? Ptcgocode { get; set; }
    }
}
