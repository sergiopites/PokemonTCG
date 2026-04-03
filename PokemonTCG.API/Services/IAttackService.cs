namespace PokemonTCG.API.Services
{
    public interface IAttackService
    {
        Task<List<Models.Attack>> GetAttacksByCardIdAsync(string cardId);
    }
}
