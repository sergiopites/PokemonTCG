using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PokemonTCG.API.Models;
namespace PokemonTCG.API.Models
{
    public class CardMarketPrice
    {
        [Key]
        public int CardMarketPriceId { get; set; }   // ✅ Necesita su propio PK

        [ForeignKey(nameof(Models.CardMarket))]
        public int CardMarketId { get; set; }
        public CardMarket CardMarket { get; set; } = null!;
        public decimal? AverageSellPrice { get; set; }
        public decimal? LowPrice { get; set; }
        public decimal? TrendPrice { get; set; }
        public decimal? ReverseHoloLow { get; set; }
        public decimal? ReverseHoloTrend { get; set; }
        public decimal? LowPriceExPlus { get; set; }
        public decimal? AverageDay { get; set; }
        public decimal? AverageWeek { get; set; }
        public decimal? AverageMonth { get; set; }
        public decimal? AverageDayReverseHolo { get; set; }
        public decimal? AverageWeekReverseHolo { get; set; }
        public decimal? AverageMonthReverseHolo { get; set; }
    }
}
