using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using PokemonTCG.API.Data;
using PokemonTCG.API.Models;

namespace PokemonTCG.API.Repositories
{
    public class LegalityRepository : ILegalityRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<LegalityRepository> _logger;
       
        public LegalityRepository(AppDbContext context, ILogger<LegalityRepository> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<List<Models.Legality>> GetAllLegalities()
        {
            try
            {
                return await _context.Legalities.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving all legalities: {ex.Message}");
                return new List<Models.Legality>();
            }
        }
        public async Task<Legality> SaveLegalityAsync(Models.Legality legality, CancellationToken cancellationToken = default)
        {
            try
            {

                bool exists = await _context.Legalities.AnyAsync(l => l.LegalityId == legality.LegalityId, cancellationToken);
                if (!exists)
                {
                    _context.Legalities.Add(legality);
                    await _context.SaveChangesAsync();
                    return legality;
                }
                else
                {
                    _logger.LogInformation($"The legality id {legality.LegalityId} exists in the database");
                }

                
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving legality with ID '{legality.LegalityId}': {ex.Message}");
            }

            return legality;
        }
    }
}
