using PokemonTCG.API.Repositories;

namespace PokemonTCG.API.Services
{
    public class CardImageService : ICardImageService
    {
        private readonly ICardImageRepository _cardImageRepository;
        private readonly ILogger _logger;
        public CardImageService(ICardImageRepository cardImageRepository, ILogger<CardImageService> logger)
        {
            _cardImageRepository = cardImageRepository;
            _logger = logger;
        }
        public void SaveImageCard(Models.CardImage cardImage, CancellationToken cancellationToken)
        {
            try
            {
                _cardImageRepository.SaveImageCardAsync(cardImage, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving card image with ID '{cardImage.CardImageId}': {ex.Message}");
            }
        }
        public async Task<List<Models.CardImage>> GetAllImagesAsync()
        {
            try
            {
                return await _cardImageRepository.GetAllImagesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving all card images: {ex.Message}");
                return new List<Models.CardImage>();
            }
        }
    }
}
