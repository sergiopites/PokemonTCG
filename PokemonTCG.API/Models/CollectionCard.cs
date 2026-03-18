namespace PokemonTCG.API.Models
{
    public class CollectionCard
    {
        public int CollectionCardId { get; set; }
        public int? CollectionId { get; set; }
        public Collection Collection { get; set; }
        public string CardId { get; set; }
        public Card Card { get; set; }
        public int Quantity { get; set; }
    }
}
