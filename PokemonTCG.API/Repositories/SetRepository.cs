using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PokemonTCG.API.Data;
using PokemonTCG.API.DTOs;
using PokemonTCG.API.Models;
using PokemonTCG.API.Responses;
using Microsoft.Extensions.Logging;

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
        public async Task<List<SetDetailDTO>> GetSetByIdAsync(string id)
        {
            try
            {
                var setList = await _context.Sets
                    .Where(s => s.SetId == id)
                    .Include(s => s.Images)
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
                _logger.LogError($"Error retrieving sets by ID '{id}': {ex.Message}");
                return new List<SetDetailDTO>();
            }
        }

        public async Task<List<SetDetailDTO>> GetSetByNameAsync(string name)
        {
            try
            {
                var setList = await _context.Sets
                    .Where(s => s.Name == name)
                    .Include(s => s.Images)
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
                _logger.LogError($"Error retrieving sets by name '{name}': {ex.Message}");
                return new List<SetDetailDTO>();
            }
        }

        public async Task<List<SetDetailDTO>> GetSetBySerieAsync(string serie)
        {
            try
            {
                var setList = await _context.Sets
                    .Where(s => s.Series == serie)
                    .Include(s => s.Images)
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
                _logger.LogError($"Error retrieving sets by serie '{serie}': {ex.Message}");
                return new List<SetDetailDTO>();
            }
        }

        public async Task<Set> SaveSetAsync(Models.Set set, CancellationToken cancellationToken = default)
        {
            try
            {
                var existing = await _context.Sets
                    .FirstOrDefaultAsync(s => s.SetId == set.SetId, cancellationToken);

                if (existing != null)
                    return existing;

                _context.Sets.Add(set);

                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation($"Set: {set.SetId} was updated successfully.");

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
