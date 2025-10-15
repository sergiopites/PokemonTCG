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
        public void SaveAbilityAsync(List<Models.Ability> ability, CancellationToken cancellationToken = default)
        {
            try
            {
                if (ability == null || ability.Count == 0)
                {
                    _logger.LogWarning("No abilities to save.");
                    return;
                }

                _abilityRepository.SaveAbilityAsync(ability, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving ability: {ex.Message}");
            }
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
