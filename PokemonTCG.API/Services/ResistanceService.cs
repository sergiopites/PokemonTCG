using PokemonTCG.API.Models;
using PokemonTCG.API.Repositories;

namespace PokemonTCG.API.Services
{    
    public class ResistanceService:IResistanceService
    {
        private readonly IResistanceRepository _resistanceRepository;
        private readonly ILogger<IResistanceService> _logger;
        public ResistanceService(ILogger<IResistanceService> logger, IResistanceRepository resistanceRepository)
        {
            _resistanceRepository = resistanceRepository;
            _logger = logger;
        }

        public async Task<List<Resistance>> GetResistanceByCardId(string cardId)
        {
            try
            { 
                return await _resistanceRepository.GetResistancesByCardIdAsync(cardId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving resistance by card Id '{cardId}': {ex.Message}");
                return new List<Models.Resistance>();
            }
        }
    }
}
