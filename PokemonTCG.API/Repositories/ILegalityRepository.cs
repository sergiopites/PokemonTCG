namespace PokemonTCG.API.Repositories
{
    public interface ILegalityRepository
    {
        Task<List<Models.Legality>> GetAllLegalities();
        Task<Models.Legality> SaveLegalityAsync(Models.Legality legality, CancellationToken cancellationToken = default);
    }
}
