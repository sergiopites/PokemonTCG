namespace PokemonTCG.API.Services
{
    public interface IAttackService
    {
        void SaveAttackAsync(List<Models.Attack> attacks, CancellationToken cancellationToken = default);
        Task<List<Models.Attack>> GetAttacksByCardIdAsync(string cardId);
    }
}
