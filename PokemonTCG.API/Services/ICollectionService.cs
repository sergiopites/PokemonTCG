using PokemonTCG.API.Request;

namespace PokemonTCG.API.Services
{
    public interface ICollectionService
    {
        Task<CollectionRequest> SaveCollectionAsync(CollectionRequest collectionRequest, CancellationToken cancellationToken = default);
        Task<List<CollectionRequest>> GetAllCollectionsAsync(CancellationToken cancellationToken = default);
        Task<CollectionRequest?> GetCollectionByIdAsync(int collectionId, CancellationToken cancellationToken = default);
    }
}
