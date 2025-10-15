using System.ComponentModel.DataAnnotations;

namespace PokemonTCG.API.Models
{
    public class SetImage
    {
        [Key]
        public int SetImageId { get; set; }
        public Uri? Symbol { get; set; }
        public Uri? Logo { get; set; }
    }
}