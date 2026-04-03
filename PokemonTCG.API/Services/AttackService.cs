using PokemonTCG.API.Models;
using PokemonTCG.API.Repositories;
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
