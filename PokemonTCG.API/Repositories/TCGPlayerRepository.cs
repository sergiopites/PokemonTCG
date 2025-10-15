using Microsoft.EntityFrameworkCore;
using PokemonTCG.API.Models;
using PokemonTCG.API.Data;
namespace PokemonTCG.API.Repositories
{
    public class TCGPlayerRepository : ITCGPlayerRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ITCGPlayerRepository> _logger;

        public TCGPlayerRepository(AppDbContext context, ILogger<TCGPlayerRepository> logger)
        {
            _context = context;
            _logger = logger;
        }
        //public async Task<TCGPlayer> SaveTCGPlayerAsync(TCGPlayer tcgPlayer, CancellationToken cancellationToken)
        //{
        //    //try
        //    //{
        //    //    var existing = await _context.TCGPlayers
        //    //        .Include(t => t.TCGPlayerPrices)
        //    //        .FirstOrDefaultAsync(tcgp =>
        //    //            tcgPlayer.Url == tcgp.Url && tcgp.TCGPlayerId == tcgPlayer.TCGPlayerId,
        //    //            cancellationToken);

        //    //    if (existing != null)
        //    //        return existing;

        //    //    // Inicializar 1:1
        //    //    tcgPlayer.TCGPlayerPrices = new ICollection<TCGPlayerPrice>();

        //    //    // Asegurarse de que Card sea consistente
        //    //    if (tcgPlayer.Card != null)
        //    //    {
        //    //        _context.Cards.Attach(tcgPlayer.Card);
        //    //    }
        //    //    else if (string.IsNullOrEmpty(tcgPlayer.CardId))
        //    //    {
        //    //        _logger.LogError("TCGPlayer debe tener Card o CardId asignado");
        //    //    }

        //    //    _context.TCGPlayers.Add(tcgPlayer);
        //    //    await _context.SaveChangesAsync(cancellationToken);

        //    //    return tcgPlayer;
        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    _logger.LogError($"Error saving tcgplayer with URL '{tcgPlayer.Url}': {ex}");
        //    //    throw; // relanzar para no perder el stack trace
        //    //}
        //}

    }
}
