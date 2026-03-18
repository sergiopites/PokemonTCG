using PokemonTCG.API.DTOs;

namespace PokemonTCG.API.Repositories
{
    public interface ICollectionRepository
    {
        Task<CollectionDetailDTO> SaveCollectionAsync(CollectionDetailDTO collectionDetailDTO, CancellationToken cancellationToken = default);
    }
}
