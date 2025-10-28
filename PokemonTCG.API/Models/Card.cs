using PokemonTCG.API.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PokemonTCG.API.Models
{
    public class Card
    {
        [Key]
        [Required]
        public string CardId { get; set; }
        [MaxLength(100)]
        public string? Name { get; set; }
        [MaxLength(30)]
        public string? SuperType { get; set; }
        public string? SubTypes { get; set; }
        public string? Level { get; set; }
        public int? Hp { get; set; }
        [MaxLength(100)]
        public string? Types { get; set; }
        public string? EvolvesFrom { get; set; }
        public ICollection<Ability> Abilities { get; set; }
        public ICollection<Attack> Attacks { get; set; }
        public ICollection<Weakness> Weaknesses { get; set; }
        public ICollection<Resistance> Resistances { get; set; }        
        public string? RetreatCost { get; set; }
        public int? ConvertedRetreatCost { get; set; }
        public string SetId { get; set; }
        public Set Set { get; set; }
        public string? Number { get; set; }
        public string? Artist { get; set; }
        [MaxLength(30)]
        public string? Rarity { get; set; }
        public string? NationalPokedexNumbers { get; set; }
        public int? LegalitiesId { get; set; }
        public Legality Legalities { get; set; }
        public int? CardImageId { get; set; }
        public CardImage CardImage { get; set; }        
        public TCGPlayer Tcgplayer { get; set; }        
        public CardMarket CardMarket { get; set; }
        public string? EvolvesTo { get; set; }
        public string? FlavorText { get; set; }
        public string? Rules { get; set; }
        public string? RegulationMark { get; set; }
        [Required]
        public string ExternalId { get; set; }  // ID de la API                
        public ICollection<DeckCard> DeckCards { get; set; } = new List<DeckCard>();
    }
}