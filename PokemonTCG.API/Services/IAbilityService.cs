namespace PokemonTCG.API.Services
{
    public interface IAbilityService
    {
        Task<List<Models.Ability>> GetAbilitiesByCardIdAsync(string cardId);
    }
}
