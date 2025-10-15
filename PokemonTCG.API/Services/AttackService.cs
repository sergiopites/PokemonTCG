using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PokemonTCG.API.Data;
using PokemonTCG.API.Models;
using PokemonTCG.API.Repositories;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
namespace PokemonTCG.API.Services
{
    public class AttackService : IAttackService
    {
        private readonly IAttackRepository _attackRepository;
        private readonly ILogger<AttackService> _logger;

        public AttackService(IAttackRepository attackRepository, ILogger<AttackService> logger)
        {
            _attackRepository = attackRepository;
            _logger = logger;
        }

        public void SaveAttackAsync(List<Models.Attack> attacks, CancellationToken cancellationToken = default)
        {
            try
            {
                if (attacks == null || attacks.Count == 0)
                {
                    _logger.LogWarning("No attacks to save.");
                    return;
                }

                _attackRepository.SaveAttackAsync(attacks, cancellationToken);

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
                return await _attackRepository.GetAttacksByCardIdAsync(cardId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting attacks from card {cardId}");
                return new List<Attack>();
            }
        }

    }
}
