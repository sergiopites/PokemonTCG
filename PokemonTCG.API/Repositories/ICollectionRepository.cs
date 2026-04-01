using PokemonTCG.API.DTOs;
using PokemonTCG.API.DTOs;

namespace PokemonTCG.API.Repositories
{
    public interface ICollectionRepository
    {
        Task<CollectionDetailDTO> SaveCollectionAsync(CollectionDetailDTO collectionDetailDTO, CancellationToken cancellationToken = default);
        Task<List<CollectionDetailDTO>> GetAllCollectionsAsync(CancellationToken cancellationToken = default);
        Task<CollectionDetailDTO?> GetCollectionByIdAsync(int collectionId, CancellationToken cancellationToken = default);
    }
}
