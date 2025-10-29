namespace PokemonTCG.API.Models
{
    public class DeckCard
    {
        public int DeckCardId { get; set; }
        public int? DeckId { get; set; }
        public Deck Deck { get; set; }        
        public string CardId { get; set; }               
        public Card Card { get; set; }
        public int Quantity { get; set; }      
    }

}
