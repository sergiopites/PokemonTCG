using Microsoft.EntityFrameworkCore;
using PokemonTCG.API.Data;
using PokemonTCG.API.Models;

namespace PokemonTCG.API.Repositories
{
    public class AncientTraitRepository : IAncientTraitRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AncientTraitRepository> _logger;
        public AncientTraitRepository(AppDbContext context, ILogger<AncientTraitRepository> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<AncientTrait> SaveAncientTraitAsync(AncientTrait ancientTraits, CancellationToken cancellationToken = default)
        {
            try
            {
                if (ancientTraits.Name != null || ancientTraits.Text != null)
                {
                    var existing = await _context.AncientTraits.FirstOrDefaultAsync(
                    at => at.Name == ancientTraits.Name && at.Text == ancientTraits.Text);

                    if (existing != null)
                        return existing;

                    _context.AncientTraits.Add(ancientTraits);
                    await _context.SaveChangesAsync();
                }


                return ancientTraits;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving AncientTrait..." + ex.Message);
                throw;
            }
        }
    }
}
