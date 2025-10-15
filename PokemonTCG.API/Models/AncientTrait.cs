using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PokemonTCG.API.Models
{
    public class AncientTrait
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AncientTraitId { get; set; }
        public string? Name { get; set; }
        public string? Text { get; set; }        
        public Card Card { get; set; }
    }
}