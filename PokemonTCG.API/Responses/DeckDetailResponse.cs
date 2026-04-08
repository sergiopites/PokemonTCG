using PokemonTCG.API.Models;

namespace PokemonTCG.API.Responses
{
    public class DeckDetailResponse
    {
        public string DeckName { get; set; }
        public string DominantType { get; set; }
        public List<string> PowerPokémon { get; set; } = new();
        public int Total { get; set; }
        public int Pokémon { get; set; }
        public int Trainers { get; set; }
        public int Energy { get; set; }
        public List<DeckAutoCardDTO> Cards { get; set; } = new();
        public class DeckAutoCardDTO
        {
            public string CardId { get; set; }
            public string ImageLarge { get; set; }
            public string Name { get; set; }
            public string SetName { get; set; }
            public string SetId { get; set; }
            public string Rule { get; set; }
            public string Subtype { get; set; }
            public string Type { get; set; }
            public string Supertype { get; set; }
            public string Number { get; set; }
            public string Ptcgocode { get; set; }
            public string Rarity { get; set; }
            public string Artist { get; set; }
            public int? HP { get; set; }
            public string EvolvesFrom { get; set; }
            public string EvolvesTo { get; set; }
        }
    }
}
