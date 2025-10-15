using Microsoft.EntityFrameworkCore;
using PokemonTCG.API.Data;
using PokemonTCG.API.Models;

namespace PokemonTCG.API.Repositories
{
    public class SetImageRepository : ISetImageRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<SetImageRepository> _logger;
        public SetImageRepository(AppDbContext context, ILogger<SetImageRepository> logger)
        {
            _context = context;
            _logger = logger;
        }
       

        public async Task<SetImage> SaveImageCardAsync(SetImage setImage, CancellationToken cancellationToken)
        {
            try
            {
                var existing = await _context.SetImages
                   .FirstOrDefaultAsync(img => img.Logo == setImage.Logo && img.Symbol == setImage.Symbol);

                if (existing != null)
                    return existing; // O su Id

                _context.SetImages.Add(setImage);
                
                await _context.SaveChangesAsync();

                return setImage;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving set image with ID '{setImage.SetImageId}': {ex.Message}");
                return setImage;
            }
        }
    }
}
