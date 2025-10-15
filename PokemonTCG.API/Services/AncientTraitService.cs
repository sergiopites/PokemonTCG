using PokemonTCG.API.Repositories;

namespace PokemonTCG.API.Services
{
    public class AncientTraitService : IAncientTraitService
    {
        private readonly IAncientTraitRepository _ancientTraitRepository;
        private readonly ILogger<AncientTraitService> _logger;
        public AncientTraitService(IAncientTraitRepository ancientTraitRepository, ILogger<AncientTraitService> logger)
        {
            _ancientTraitRepository = ancientTraitRepository;
            _logger = logger;
        }

        public void SaveAntientTraitAsync(Models.AncientTrait ancientTraits, CancellationToken cancellationToken = default)
        {
            try
            {
                try
                {
                    if (ancientTraits == null)
                    {
                        _logger.LogWarning("No ancientTraits to save.");
                        return;
                    }

                    _ancientTraitRepository.SaveAncientTraitAsync(ancientTraits, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error saving ancientTraits: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving ancientTraits..." + ex.Message);
                throw;
            }
        }
    }
}
