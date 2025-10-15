using Microsoft.EntityFrameworkCore;
using PokemonTCG.API.Data;
using PokemonTCG.API.Models;

namespace PokemonTCG.API.Repositories
{
    public class AbilityRepository : IAbilityRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AbilityRepository> _logger;

        public AbilityRepository(AppDbContext context, ILogger<AbilityRepository> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<List<Ability>> SaveAbilityAsync(List<Models.Ability> abilities, CancellationToken cancellationToken = default)
        {
            try
            {
                foreach (var ab in abilities)
                {
                    // Verifica si ya existe una carta con el mismo Id
                    bool exists = await _context.Abilities.AnyAsync(a => a.AbilityId == ab.AbilityId, cancellationToken);
                    if (!exists)
                    {
                        _context.Abilities.Add(ab);
                        _logger.LogInformation($"Ability with ID {ab.AbilityId} saved successfully.");
                    }
                    else
                    {
                        _logger.LogInformation($"The ability id {ab.AbilityId} exists in the database");
                    }

                    await _context.SaveChangesAsync(cancellationToken);
                }
                return abilities;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving abilities..." + ex.Message);
                throw;
            }
        }

        public async Task<List<Models.Ability>> GetAbilitiesByCardIdAsync(string cardId)
        {
            try
            {
                var listAbilitiesByCard =  _context
                                           .Abilities
                                           .Where(x => x.CardId == cardId)
                                           .ToList();

                return listAbilitiesByCard;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting abilities from card {cardId}");
                return new List<Ability>();
            }
        }
    }
}
