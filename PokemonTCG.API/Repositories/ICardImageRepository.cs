using PokemonTCG.API.Models;

namespace PokemonTCG.API.Repositories
{
    public interface ICardImageRepository
    {
        Task<CardImage> SaveImageCardAsync(CardImage cardImage, CancellationToken cancellationToken);
        Task<List<CardImage>> GetAllImagesAsync();
    }
}
