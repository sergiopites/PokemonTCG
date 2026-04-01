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

        public async Task<List<CollectionRequest>> GetAllCollectionsAsync(CancellationToken cancellationToken = default)
        {
            var collections = await _collectionRepository.GetAllCollectionsAsync(cancellationToken);

            return collections.Select(c => new CollectionRequest
            {
                CollectionId = c.CollectionId,
                Name = c.Name,
                Description = c.Description,
                Cards = c.Cards.Select(cc => new CollectionCardQuantityRequest
                {
                    CardId = cc.CardId,
                    Quantity = cc.Quantity
                }).ToList()
            }).ToList();
        }

        public async Task<CollectionRequest?> GetCollectionByIdAsync(int collectionId, CancellationToken cancellationToken = default)
        {
            var collection = await _collectionRepository.GetCollectionByIdAsync(collectionId, cancellationToken);

            if (collection == null)
                return null;

            return new CollectionRequest
            {
                CollectionId = collection.CollectionId,
                Name = collection.Name,
                Description = collection.Description,
                Cards = collection.Cards.Select(cc => new CollectionCardQuantityRequest
                {
                    CardId = cc.CardId,
                    Quantity = cc.Quantity,
                    Name = cc.Name,
                    ImageLarge = cc.ImageLarge,
                    Supertype = cc.Supertype,
                    Subtype = cc.Subtype,
                    Number = cc.Number,
                    SetName = cc.SetName,
                    SetId = cc.SetId,
                    Ptcgocode = cc.Ptcgocode
                }).ToList()
            };
        }
    }
}
