namespace PokemonTCG.API.Models
{
    public class Deck
    {
        public int DeckId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ICollection<DeckCard> DeckCards { get; set; } = new List<DeckCard>();
    }
}
