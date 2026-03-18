using PokemonTCG.API.DTOs;
using PokemonTCG.API.Repositories;
using PokemonTCG.API.Request;

namespace PokemonTCG.API.Services
{
    public class CollectionService : ICollectionService
    {
        private readonly ILogger<ICollectionRepository> _logger;
        private readonly ICollectionRepository _collectionRepository;

        public CollectionService(ILogger<ICollectionRepository> logger, ICollectionRepository collectionRepository)
        {
            _logger = logger;
            _collectionRepository = collectionRepository;
        }

        public async Task<CollectionRequest> SaveCollectionAsync(CollectionRequest collectionRequest, CancellationToken cancellationToken = default)
        {
            var collectionDTO = new CollectionDetailDTO
            {
                CollectionId = collectionRequest.CollectionId,
                Name = collectionRequest.Name,
                Description = collectionRequest.Description,
                Cards = collectionRequest.Cards.Select(c => new CollectionCardDTO
                {
                    CardId = c.CardId,
                    Quantity = c.Quantity
                }).ToList()
            };

            collectionDTO = await _collectionRepository.SaveCollectionAsync(collectionDTO, cancellationToken);

            var result = new CollectionRequest
            {
                CollectionId = collectionDTO.CollectionId,
                Name = collectionDTO.Name,
                Description = collectionDTO.Description,
                Cards = collectionDTO.Cards.Select(c => new CollectionCardQuantityRequest
                {
                    CardId = c.CardId,
                    Quantity = c.Quantity
                }).ToList()
            };

            return result;
        }
    }
}
