using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;

namespace PokemonTCG.API.Models
{
    public class Price
    {
        [Key]
        public int PriceId { get; set; }
        [ForeignKey(nameof(TCGPlayerPrice))]
        public int TCGPlayerPriceId { get; set; }
        public TCGPlayerPrice TCGPlayerPrice { get; set; } = null!;
        public PriceType Type { get; set; }
        public double? Low { get; set; }
        public double? Mid { get; set; }
        public double? High { get; set; }
        public double? Market { get; set; }
        public double? DirectLow { get; set; }
    }

    public enum PriceType { 
        Holofoil = 1, 
        ReverseHolofoil = 2, 
        Normal = 3, 
        FirstEditionHolofoil = 4, 
        UnlimitedHolofoil = 5, 
        FirstEdition = 6, 
        Unlimited = 7 
    }
}