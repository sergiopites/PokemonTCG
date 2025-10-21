using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PokemonTCG.API.Models
{
    public class Set
    {
        [Key]
        [Required]
        public string SetId { get; set; }
        [MaxLength(100)]
        public string? Name { get; set; }
        [MaxLength(100)]
        public string? Series { get; set; }
        public long? PrintedTotal { get; set; }
        public long? Total { get; set; }
        [ForeignKey("LegalitiesId")]
        public int? LegalitiesId { get; set; }
        public virtual Legality Legalities { get; set; }
        [MaxLength(20)]
        public string? PtcgoCode { get; set; }
        public string? ReleaseDate { get; set; }
        public string? UpdatedAt { get; set; }

        [ForeignKey("ImagesId")]
        public int? ImagesId { get; set; }
        public virtual SetImage Images { get; set; }
    }
}