using Microsoft.EntityFrameworkCore;
using PokemonTCG.API.Data;
using PokemonTCG.API.Models;

namespace PokemonTCG.API.Repositories
{
    public class ResistanceRepository : IResistanceRepository   
    {
        private readonly ILogger _logger;
        private readonly AppDbContext _appDbContext;
        public ResistanceRepository(ILogger<ResistanceRepository> logger, AppDbContext appDbContext)
        {
            _logger = logger;
            _appDbContext = appDbContext;
        }

        public async Task<List<Resistance>> GetResistancesByCardIdAsync(string cardId)
        {
            try
            {
                return await _appDbContext.Resistances.Where(r => r.CardId == cardId).ToListAsync();

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving resistance by card Id '{cardId}': {ex.Message}");
                return new List<Models.Resistance>();
            }
        }
    }
}
