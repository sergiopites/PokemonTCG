using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PokemonTCG.API.Models
{
    public class CardMarket
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CardMarketId { get; set; }
        public Uri? Url { get; set; }
        public string? UpdatedAt { get; set; }
        public string CardId { get; set; }
        public Card Card { get; set; } = null!;        
        public ICollection<CardMarketPrice> CardMarketPrices { get; set; } = null!;
    }
}