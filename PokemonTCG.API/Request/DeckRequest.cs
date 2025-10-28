using PokemonTCG.API.Models;

namespace PokemonTCG.API.Request
{
    public class DeckRequest
    {
        public int? DeckId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ICollection<CardQuantityRequest> Cards { get; set; } = new List<CardQuantityRequest>();
    }

    public class CardQuantityRequest
    {
        public string CardId { get; set; }
        public int Quantity { get; set; }
    }
}
