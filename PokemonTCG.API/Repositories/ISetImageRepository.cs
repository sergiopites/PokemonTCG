using PokemonTCG.API.Models;

namespace PokemonTCG.API.Repositories
{
    public interface ISetImageRepository
    {
        Task<SetImage> SaveImageCardAsync(SetImage setImage, CancellationToken cancellationToken);
    }
}
