using Microsoft.EntityFrameworkCore;
using PokemonTCG.API.Data;
using PokemonTCG.API.Models;
using System.Linq.Expressions;

namespace PokemonTCG.API.Repositories
{
    public class CardImageRepository : ICardImageRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CardImageRepository> _logger;

        public CardImageRepository(AppDbContext context, ILogger<CardImageRepository> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<CardImage> SaveImageCardAsync(CardImage cardImage, CancellationToken cancellationToken)
        {
            try
            {
                var existing = await _context.CardImages
                   .FirstOrDefaultAsync(img => img.Small == cardImage.Small && img.Large == cardImage.Large);

                if (existing != null)
                    return existing; // O su Id

                _context.CardImages.Add(cardImage);
                await _context.SaveChangesAsync();
                return cardImage;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving card image with ID '{cardImage.CardImageId}': {ex.Message}");
            }

            return cardImage;
        }
        public async Task<List<CardImage>> GetAllImagesAsync()
        {
            try
            {
                return await _context.CardImages.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving all card images: {ex.Message}");
                return new List<CardImage>();
            }
        }
    }
}
