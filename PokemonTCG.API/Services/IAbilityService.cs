namespace PokemonTCG.API.Services
{
    public interface IAbilityService
    {
        void SaveAbilityAsync(List<Models.Ability> ability, CancellationToken cancellationToken = default);
        Task<List<Models.Ability>> GetAbilitiesByCardIdAsync(string cardId);
    }
}
