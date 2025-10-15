using System.ComponentModel.DataAnnotations;

namespace PokemonTCG.API.Models
{
    public class Attack
    {
        [Key]
        public int AttackId { get; set; }
        public string? Name { get; set; }
        public string? Text { get; set; }
        public string? Damage { get; set; }
        public string? ConvertedEnergyCost { get; set; }
        public string? CostJson { get; set; }
        public string? CardId { get; set; }
        public Card Card { get; set; }
    }
}