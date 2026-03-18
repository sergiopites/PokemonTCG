using PokemonTCG.API.Request;

namespace PokemonTCG.API.Services
{
    public interface ICollectionService
    {
        Task<CollectionRequest> SaveCollectionAsync(CollectionRequest collectionRequest, CancellationToken cancellationToken = default);
    }
}
