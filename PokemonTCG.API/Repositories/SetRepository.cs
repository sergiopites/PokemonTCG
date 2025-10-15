using Microsoft.EntityFrameworkCore;
using PokemonTCG.API.Data;
using PokemonTCG.API.DTOs;
using PokemonTCG.API.Models;
using System.Collections.Generic;

namespace PokemonTCG.API.Repositories
{
    public class SetRepository : ISetRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<SetRepository> _logger;
        public SetRepository(AppDbContext context, ILogger<SetRepository> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<List<SetDetailDTO>> GetAllSetsAsync()
        {
            try
            {
                var setList = await _context.Sets
                    .Include(s => s.Images)
                    .OrderByDescending(s => s.ReleaseDate)
                    //.OrderBy(s=>s.ReleaseDate)
                    .Select(s => new SetDetailDTO
                    {
                        SetId = s.SetId,
                        Name = s.Name,
                        Series = s.Series,
                        PrintedTotal = s.PrintedTotal,
                        Total = s.Total,
                        PtcgoCode = s.PtcgoCode,
                        ReleaseDate = s.ReleaseDate,
                        UpdatedAt = s.UpdatedAt,
                        Logo = s.Images.Logo,
                        Symbol = s.Images.Symbol
                    }).ToListAsync();
                return setList;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving all sets: {ex.Message}");
                return new List<SetDetailDTO>();
            }
        }
        public async Task<List<Models.Set>> GetSetByIdAsync(string id)
        {
            try
            {
                return await _context.Sets.Where(c => c.SetId == id).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving sets by ID '{id}': {ex.Message}");
                return new List<Models.Set>();
            }
        }
        public async Task<List<Models.Set>> GetSetByNameAsync(string name)
        {
            try
            {
                return await _context.Sets.Where(c => c.Name == name).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving sets by name '{name}': {ex.Message}");
                return new List<Models.Set>();
            }
        }
        public async Task<List<Models.Set>> GetSetBySerieAsync(string serie)
        {
            try
            {
                return await _context.Sets.Where(c => c.Series == serie).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving sets by serie '{serie}': {ex.Message}");
                return new List<Models.Set>();
            }
        }
        public async Task<Set> SaveSetAsync(Models.Set set, CancellationToken cancellationToken = default)
        {
            try
            {
                var existing = await _context.Sets
                    .FirstOrDefaultAsync(s => s.SetId == set.SetId, cancellationToken);

                if (existing != null)
                    return existing; // O su Id

                _context.Sets.Add(set);
                await _context.SaveChangesAsync();

                return set;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving sets.");
                throw;
            }
        }

    }
}
