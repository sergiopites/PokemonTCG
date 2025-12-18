using System;
using System.Collections.Generic;
using System.Linq;
using PokemonTCG.API.DTOs;
using PokemonTCG.API.Models;
using PokemonTCG.API.Repositories;
using PokemonTCG.API.Responses;
using PokemonTCG.SDK.Infrastructure.HttpClients;
using PokemonTCG.SDK.Infrastructure.HttpClients.Set;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace PokemonTCG.API.Services
{
    public class SetService : ISetService
    {
        private readonly ISetRepository _setRepository;
        private readonly ILegalityRepository _legalityRepository;
        private readonly ILogger _logger;
        private readonly ISetImageRepository _setImageRepository;
        public SetService(ISetRepository setRepository, ILogger<SetService> logger, ILegalityRepository legalityRepository, ISetImageRepository setImageRepository)
        {
            _setRepository = setRepository;
            _logger = logger;
            _legalityRepository = legalityRepository;
            _setImageRepository = setImageRepository;
        }
        public async Task SaveSetAsync(CancellationToken cancellationToken)
        {
            try
            {
                // Obtener los sets desde la API externa
                var externalSets = await GetAllPokemonSetsAsync();
                if (externalSets == null || !externalSets.Any())
                    return;

                foreach (var s in externalSets)
                {
                    var setImage = new SetImage()
                    {
                        Logo = s.Images?.Logo,
                        Symbol = s.Images?.Symbol,
                    };

                    setImage = await _setImageRepository.SaveImageCardAsync(setImage, cancellationToken);

                    var setLegality = new Legality()
                    {
                        Expanded = s.Legalities?.Expanded,
                        Standard = s.Legalities?.Standard,
                        Unlimited = s.Legalities?.Unlimited
                    };

                    setLegality = await _legalityRepository.SaveLegalityAsync(setLegality, cancellationToken);

                    var set = new Models.Set()
                    {
                        SetId = s.SetId,
                        Name = s.Name ?? null,
                        Series = s.Series ?? null,
                        PrintedTotal = s.PrintedTotal ?? null,
                        Total = s.Total ?? null,
                        LegalitiesId = setLegality.LegalityId,
                        PtcgoCode = s.PtcgoCode ?? null,
                        ReleaseDate = s.ReleaseDate ?? null,
                        UpdatedAt = s.UpdatedAt ?? null,
                        ImagesId = setImage.SetImageId
                    };

                    await _setRepository.SaveSetAsync(set, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving sets: {ex.Message}");
            }

        }
        public async Task<List<SetDetailResponse>> GetAllSetsAsync()
        {
            try
            {
                List<SetDetailDTO> setDetailDTO = await _setRepository.GetAllSetsAsync();

                var responseList = setDetailDTO.Select(dto => new SetDetailResponse
                {
                    Logo = dto.Logo,
                    Name = dto.Name,
                    PrintedTotal = dto.PrintedTotal,
                    PctgoCode = dto.PtcgoCode,
                    ReleaseDate = dto.ReleaseDate,
                    SetId = dto.SetId,
                    Series = dto.Series,
                    Symbol = dto.Symbol,
                    Total = dto.Total,
                    UpdatedAt = dto.UpdatedAt

                }).ToList();

                return responseList;

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving all sets: {ex.Message}");
                return new List<SetDetailResponse>();
            }

        }
        public async Task<List<SetDetailResponse>> GetSetByIdAsync(string id)
        {
            try
            {
                // Obtener los sets del repositorio
                var sets = await _setRepository.GetSetByIdAsync(id);

                if (sets == null || !sets.Any())
                    return new List<SetDetailResponse>();

                // Mapear a SetDetailResponse
                var responseList = sets.Select(s => new SetDetailResponse
                {
                    Logo = s.Logo,
                    Symbol = s.Symbol,
                    Name = s.Name,
                    PrintedTotal = s.PrintedTotal,
                    PctgoCode = s.PtcgoCode,
                    ReleaseDate = s.ReleaseDate,
                    SetId = s.SetId,
                    Series = s.Series,
                    Total = s.Total,
                    UpdatedAt = s.UpdatedAt
                }).ToList();

                return responseList;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving sets by ID '{id}': {ex.Message}");
                return new List<SetDetailResponse>();
            }
        }
        public async Task<List<SetDetailResponse>> GetSetByNameAsync(string name)
        {
            try
            {
                var dtos = await _setRepository.GetSetByNameAsync(name);

                if (dtos == null || !dtos.Any())
                    return new List<SetDetailResponse>();

                var responseList = dtos.Select(dto => new SetDetailResponse
                {
                    Logo = dto.Logo,
                    Symbol = dto.Symbol,
                    Name = dto.Name,
                    PrintedTotal = dto.PrintedTotal,
                    PctgoCode = dto.PtcgoCode,
                    ReleaseDate = dto.ReleaseDate,
                    SetId = dto.SetId,
                    Series = dto.Series,
                    Total = dto.Total,
                    UpdatedAt = dto.UpdatedAt
                }).ToList();

                return responseList;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving sets by name '{name}': {ex.Message}");
                return new List<SetDetailResponse>();
            }
        }
        public async Task<List<SetDetailResponse>> GetSetBySerieAsync(string serie)
        {
            try
            {
                var dtos = await _setRepository.GetSetBySerieAsync(serie);

                if (dtos == null || !dtos.Any())
                    return new List<SetDetailResponse>();

                var responseList = dtos.Select(dto => new SetDetailResponse
                {
                    Logo = dto.Logo,
                    Symbol = dto.Symbol,
                    Name = dto.Name,
                    PrintedTotal = dto.PrintedTotal,
                    PctgoCode = dto.PtcgoCode,
                    ReleaseDate = dto.ReleaseDate,
                    SetId = dto.SetId,
                    Series = dto.Series,
                    Total = dto.Total,
                    UpdatedAt = dto.UpdatedAt
                }).ToList();

                return responseList;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving sets by serie '{serie}': {ex.Message}");
                return new List<SetDetailResponse>();
            }
        }
        public async Task<List<Models.Set>> GetAllPokemonSetsAsync()
        {
            var apiKey = "9e6b5ba1-0b91-46de-89fc-740efcccfb40";

            using var client = new PokemonApiClient(apiKey);

            // Usar un DTO que herede de ApiResource
            var resourceList = await client.GetApiResourceAsync<PokemonSetApiResource>();

            if (resourceList?.Results != null && resourceList.Results.Any())
            {
                var listSet = resourceList.Results.Select(s => new Models.Set
                {
                    SetId = s.Id,
                    Name = s.Name,
                    Series = s.Series,
                    PrintedTotal = s.PrintedTotal,
                    Total = s.Total,
                    PtcgoCode = s.PtcgoCode,
                    ReleaseDate = s.ReleaseDate,
                    UpdatedAt = s.UpdatedAt,
                    Legalities = s.Legalities != null ? new Legality
                    {
                        Expanded = s.Legalities.Expanded,
                        Standard = s.Legalities.Standard,
                        Unlimited = s.Legalities.Unlimited
                    } : null,
                    Images = s.Images != null ? new SetImage
                    {
                        // Convertir strings a Uri de forma segura
                        Logo = ToUri(s.Images.Logo),
                        Symbol = ToUri(s.Images.Symbol)
                    } : null,
                }).ToList();

                return listSet;
            }

            return new List<Models.Set>();
        }

        // Método auxiliar para convertir string? -> Uri?
        private static Uri? ToUri(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            return Uri.TryCreate(url, UriKind.Absolute, out var uri) ? uri : null;
        }

        // Agregar el DTO que hereda de ApiResource
        public class PokemonSetApiResource : ApiResource
        {
            // Implementación requerida por ApiResource / ResourceBase
            public override string Id { get; set; }

            public string Name { get; set; }
            public string Series { get; set; }
            public long? PrintedTotal { get; set; }
            public long? Total { get; set; }
            public string PtcgoCode { get; set; }
            public string ReleaseDate { get; set; }
            public string UpdatedAt { get; set; }
            public LegalityDTO Legalities { get; set; }
            public SetImageDTO Images { get; set; }
        }     
    }
}