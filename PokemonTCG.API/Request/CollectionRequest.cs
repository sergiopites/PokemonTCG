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
    }
}
