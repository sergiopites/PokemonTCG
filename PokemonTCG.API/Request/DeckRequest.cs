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
        public string? Name { get; set; }
        public string? ImageLarge { get; set; }
        public string? Supertype { get; set; }
        public string? Subtype { get; set; }
        public string? Number { get; set; }
        public string? Ptcgocode { get; set; }
    }
}
