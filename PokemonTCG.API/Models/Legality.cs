using System.ComponentModel.DataAnnotations;

namespace PokemonTCG.API.Models
{
    public class Legality
    {
        [Key]
        public int LegalityId { get; set; }
        public string? Unlimited { get; set; }
        public string? Standard { get; set; }
        public string? Expanded { get; set; }
    }
}