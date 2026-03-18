using Microsoft.EntityFrameworkCore;
using PokemonTCG.API.Data;
using PokemonTCG.API.DTOs;
using PokemonTCG.API.Models;

namespace PokemonTCG.API.Repositories
{
    public class CollectionRepository : ICollectionRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ICollectionRepository> _logger;

        public CollectionRepository(ILogger<ICollectionRepository> logger, AppDbContext context)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<CollectionDetailDTO> SaveCollectionAsync(CollectionDetailDTO collectionDetailDTO, CancellationToken cancellationToken = default)
        {
            try
            {
                var collection = await _context.Collections
                    .Include(c => c.CollectionCards)
                    .FirstOrDefaultAsync(c => c.CollectionId == collectionDetailDTO.CollectionId, cancellationToken);

                if (collection == null)
                {
                    collection = new Collection
                    {
                        Name = collectionDetailDTO.Name,
                        Description = collectionDetailDTO.Description
                    };
                    _context.Collections.Add(collection);
                }
                else
                {
                    collection.Name = collectionDetailDTO.Name;
                    collection.Description = collectionDetailDTO.Description;

                    _context.CollectionCards.RemoveRange(collection.CollectionCards);
                    collection.CollectionCards.Clear();
                }

                foreach (var cardDto in collectionDetailDTO.Cards)
                {
                    collection.CollectionCards.Add(new CollectionCard
                    {
                        Collection = collection,
                        CardId = cardDto.CardId,
                        Quantity = cardDto.Quantity
                    });
                }

                await _context.SaveChangesAsync(cancellationToken);

                collectionDetailDTO.CollectionId = collection.CollectionId;
                return collectionDetailDTO;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving collection.");
                throw;
            }
        }
    }
}
