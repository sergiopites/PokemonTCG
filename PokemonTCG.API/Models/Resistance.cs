using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PokemonTCG.API.Models
{
    public class Resistance
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ResistanceId { get; set; }
        public string? Type { get; set; }
        public string? Value { get; set; }
        public string? CardId { get; set; }
        public Card Card { get; set; }
    }
}