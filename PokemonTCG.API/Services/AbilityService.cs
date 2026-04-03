using PokemonTCG.API.Models;
using PokemonTCG.API.Repositories;

namespace PokemonTCG.API.Services
{
    public class AbilityService : IAbilityService
    {
        private readonly IAbilityRepository _abilityRepository;
        private readonly ILogger<AbilityService> _logger;
        public AbilityService(IAbilityRepository abilityRepository, ILogger<AbilityService> logger)
        {
            _abilityRepository = abilityRepository;
            _logger = logger;
        }

        public async Task<List<Models.Ability>> GetAbilitiesByCardIdAsync(string cardId)
        {
            try
            {
                return await _abilityRepository.GetAbilitiesByCardIdAsync(cardId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting attacks from card {cardId}");
                return new List<Ability>();
            }
        }
    }
}
