using PokemonTCG.API.Models;

namespace PokemonTCG.API.Responses
{
    public class DeckDetailResponse
    {
        public int DeckId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public ICollection<DeckCard> Cards { get; set; }
    }
}
