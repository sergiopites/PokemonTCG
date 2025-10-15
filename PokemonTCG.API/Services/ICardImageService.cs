using PokemonTCG.API.Models;
namespace PokemonTCG.API.Services
{
    public interface ICardImageService
    {
        void SaveImageCard(CardImage cardImage, CancellationToken cancellationToken);
        Task<List<CardImage>> GetAllImagesAsync();
    }
}
