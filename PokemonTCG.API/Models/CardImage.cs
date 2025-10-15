using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PokemonTCG.API.Models
{
    public class CardImage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CardImageId { get; set; }
        public Uri? Small { get; set; }
        public Uri? Large { get; set; }
        public Card Card { get; set; }
    }
}
