namespace PokemonTCG.Models
{
    public class CardSet
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Logo { get; set; }
        public string Symbol { get; set; }
        public CardCount CardCount { get; set; }
    }

    public class CardCount
    {
        public int Total { get; set; }
        public int Official { get; set; }
    }
}
