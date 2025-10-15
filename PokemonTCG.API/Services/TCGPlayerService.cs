using PokemonTCG.API.Repositories;

namespace PokemonTCG.API.Services
{
    public class TCGPlayerService : ITCGPlayerService
    {
        private readonly ILogger<TCGPlayerService> _logger;
        private readonly ITCGPlayerRepository _tcgPlayerRepository;
        public TCGPlayerService(ILogger<TCGPlayerService> logger, ITCGPlayerRepository tcgPlayerRepository)
        {
            _logger = logger;
            _tcgPlayerRepository = tcgPlayerRepository;
        }

        public async Task SaveTCGPlayerAsync(Models.TCGPlayer tcgPlayer, CancellationToken cancellationToken)
        {
            //try
            //{
            //    await _tcgPlayerRepository.SaveTCGPlayerAsync(tcgPlayer, cancellationToken);
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError($"Error in SaveTCGPlayerAsync: {ex.Message}");
            //}
        }
    }
}
