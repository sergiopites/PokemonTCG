using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PokemonTCG.API.Models
{
    public class TCGPlayer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TCGPlayerId { get; set; }
        public Uri? Url { get; set; }
        public string? UpdatedAt { get; set; }
        public string CardId { get; set; }
        public Card Card { get; set; } = null!;
        public ICollection<TCGPlayerPrice> TCGPlayerPrices { get; set; } = null!;
    }
}