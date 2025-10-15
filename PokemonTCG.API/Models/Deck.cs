namespace PokemonTCG.API.Models
{
    public class Deck
    {
        public int DeckId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public ICollection<Card> Cards {get;set;}


    }
}
