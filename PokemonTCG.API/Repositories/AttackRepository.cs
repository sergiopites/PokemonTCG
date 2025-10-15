using Microsoft.EntityFrameworkCore;
using PokemonTCG.API.Data;
using PokemonTCG.API.Models;
using System.Threading;
using System.Threading.Tasks;

namespace PokemonTCG.API.Repositories
{
    public class AttackRepository : IAttackRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AttackRepository> _logger;

        public AttackRepository(AppDbContext context, ILogger<AttackRepository> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<List<Attack>> SaveAttackAsync(List<Models.Attack> attacks, CancellationToken cancellationToken = default)
        {
            try
            {
                foreach (var at in attacks)
                {
                    // Verifica si ya existe una carta con el mismo Id
                    bool exists = await _context.Attacks.AnyAsync(a => a.AttackId == at.AttackId, cancellationToken);
                    if (!exists)
                    {
                        _context.Attacks.Add(at);
                        _logger.LogInformation($"Attack with Id {at.AttackId} saved successfully.");
                    }
                    else
                    {
                        _logger.LogInformation($"The attack id {at.AttackId} exists in the database");
                    }

                    await _context.SaveChangesAsync(cancellationToken);
                }
                return attacks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving attacks..." + ex.Message);
                throw;
            }
        }

        public async Task<List<Models.Attack>> GetAttacksByCardIdAsync(string cardId)
        {
            try
            {
                var listAttacksByCard = _context
                                        .Attacks
                                        .Where(x => x.CardId == cardId)
                                        .ToList();
                return listAttacksByCard;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting attacks from card {cardId}");
                return new List<Attack>();
            }
        }
    }
}