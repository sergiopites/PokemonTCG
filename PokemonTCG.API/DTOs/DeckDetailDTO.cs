using PokemonTCG.API.Models;

namespace PokemonTCG.API.DTOs
{
    public class DeckDetailDTO
    {
        public int? DeckId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<DeckCardDTO> Cards { get; set; } = new();
    }

    public class DeckCardDTO
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
