namespace PokemonTCG.API.Models
{
    public class Collection
    {
        public int CollectionId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<CollectionCard> CollectionCards { get; set; } = new List<CollectionCard>();
    }
}
