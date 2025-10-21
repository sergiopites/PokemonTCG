using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PokemonTCG.API.Models
{
    public class Ability
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AbilityId { get; set; }
        public string? Name { get; set; }
        public string? Text { get; set; }
        public string? Type { get; set; }
        public string? CardId { get; set; }
        public Card Card { get; set; }
    }
}