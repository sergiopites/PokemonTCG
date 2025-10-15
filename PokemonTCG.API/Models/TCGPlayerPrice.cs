using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PokemonTCG.API.Models
{
    public class TCGPlayerPrice
    {
        [Key]
        public int TCGPlayerPriceId { get; set; }  // ✅ PK propio
        [ForeignKey(nameof(TCGPlayer))]
        public int TCGPlayerId { get; set; }
        public TCGPlayer TCGPlayer { get; set; } = null!;
        public ICollection<Price> Prices { get; set; } = new List<Price>();
    }

}