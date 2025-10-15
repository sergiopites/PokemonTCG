using PokemonTCG.API.Models;
using System.Threading.Tasks;

namespace PokemonTCG.API.Repositories
{
    public interface IAttackRepository
    {
        Task<List<Attack>> SaveAttackAsync(List<Models.Attack> attacks, CancellationToken cancellationToken = default);
        Task<List<Models.Attack>> GetAttacksByCardIdAsync(string cardId);
    }
}
