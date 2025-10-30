using Microsoft.EntityFrameworkCore;
using PokemonTCG.API.DTOs;
using PokemonTCG.API.Helpers;
using PokemonTCG.API.Models;
using PokemonTCG.API.Repositories;
using PokemonTCG.API.Responses;
using PokemonTCG.SDK.Features.FilterBuilder.Pokemon;
using PokemonTCG.SDK.Infrastructure.HttpClients;
using PokemonTCG.SDK.Infrastructure.HttpClients.Base;
using PokemonTCG.SDK.Infrastructure.HttpClients.Cards;
using Polly;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;


namespace PokemonTCG.API.Services
{
    public class CardService : ICardService
    {
        private readonly ICardRepository _cardRepository;
        private readonly ILogger<ICardService> _logger;
        private readonly ISetRepository _setRepository;
        private readonly ICardImageRepository _cardImageRepository;
        private readonly ILegalityRepository _legalityRepository;
        private readonly IAbilityRepository _abilityRepository;
        private readonly IAttackRepository _attackRepository;
        private readonly ITCGPlayerRepository _tcgPlayerRepository;
        private readonly IAncientTraitRepository _ancientTraitRepository;
        public CardService(ICardRepository cardRepository, ILogger<CardService> logger, ISetRepository setRepository, ICardImageRepository cardImageRepository, ILegalityRepository legalityRepository, IAbilityRepository abilityRepository, IAttackRepository attackRepository, ITCGPlayerRepository tcgPlayerRepository, IAncientTraitRepository ancientTraitRepository)
        {
            _cardRepository = cardRepository;
            _logger = logger;
            _setRepository = setRepository;
            _cardImageRepository = cardImageRepository;
            _legalityRepository = legalityRepository;
            _abilityRepository = abilityRepository;
            _attackRepository = attackRepository;
            _tcgPlayerRepository = tcgPlayerRepository;
            _ancientTraitRepository = ancientTraitRepository;
        }
        public async Task<List<Models.Card>> GetCardsByNumberAsync(string number)
        {
            return await _cardRepository.GetCardsByNumberAsync(number);
        }
        public async Task<List<CardDetailResponse>> GetCardByCardIdAsync(string id)
        {
            try
            {
                List<CardDetailDTO> cardDetailDTO = await _cardRepository.GetCardsByCardIdAsync(id);

                var responseList = cardDetailDTO.Select(dto => new CardDetailResponse
                {
                    CardId = dto.CardId,
                    ImageLarge = dto.ImageLarge,
                    Name = dto.Name,
                    SetName = dto.SetName,
                    SetId = dto.SetId,
                    Rule = dto.Rule,
                    Number = dto.Number,
                    Subtype = dto.Subtype,
                    Type = dto.Type,
                    Supertype = dto.Supertype,
                    CardMarketUrl = dto.CardMarketUrl,
                    TcgPlayerUrl = dto.TcgPlayerUrl,
                    Rarity = dto.Rarity,
                    Artist = dto.Artist,
                    EvolvesFrom = dto.EvolvesFrom,
                    EvolvesTo = dto.EvolvesTo,
                    HP = dto.HP,
                    SetTotal = dto.SetTotal,
                    SetImage = dto.SetImage,
                    SetSymbol = dto.SetSymbol,
                    ConvertTreatCost = dto.ConvertTreatCost,
                    RetreatCost = dto.RetreatCost,
                    ResistanceDetails = dto.ResistanceDetails.Select(r => new ResistanceDetailResponse
                    {
                        Type = r.Type,
                        Value = r.Value
                    }).ToList() ?? new List<ResistanceDetailResponse>(),

                    WeaknessDetails = dto.WeaknessDetails.Select(w => new WeaknessDetailResponse
                    {
                        Type = w.Type,
                        Value = w.Value
                    }).ToList() ?? new List<WeaknessDetailResponse>(),
                    AttackDetails = dto.AttackDetails.Select(a => new AttackDetailResponse
                    {
                        AttackName = a.AttackName,
                        AttackCost = a.AttackCost,
                        AttackDescription = a.AttackDescription,
                        AttackDamage = a.AttackDamage,
                    }).ToList() ?? new List<AttackDetailResponse>(),

                    AbilityDetails = dto.AbilityDetails.Select(b => new AbilityDetailResponse
                    {

                        Text = b.Text,
                        Type = b.Type,
                        Name = b.Name

                    }).ToList() ?? new List<AbilityDetailResponse>(),
                }).ToList();

                return responseList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new List<CardDetailResponse>();
            }
        }
        public async Task<List<Models.Card>> GetCardsBySuperTypeAsync(string supertype)
        {
            return await _cardRepository.GetCardsBySuperTypeAsync(supertype);
        }
        public async Task<List<CardDetailResponse>> GetCardsBySet(string setId)
        {
            try
            {
                List<CardDetailDTO> cardDetailDTO = await _cardRepository.GetCardsBySet(setId);

                var responseList = cardDetailDTO.Select(dto => new CardDetailResponse
                {
                    CardId = dto.CardId,
                    ImageLarge = dto.ImageLarge,
                    Name = dto.Name,
                    SetName = dto.SetName,
                    SetId = dto.SetId,
                    SetImage = dto.SetImage,
                    Rule = dto.Rule,
                    Number = dto.Number,
                    Subtype = dto.Subtype,
                    Type = dto.Type,
                    Supertype = dto.Supertype,
                    CardMarketUrl = dto.CardMarketUrl,
                    Rarity = dto.Rarity,
                    SetSerie = dto.SetSerie,
                    SetSymbol = dto.SetSymbol,
                    SetTotal = dto.SetPrintedTotal,
                    Ptcgocode = dto.Ptcgocode,
                    SetPrintedTotal = dto.SetTotal,
                    ReleaseDate = dto.ReleaseDate,
                    AttackDetails = (dto.AttackDetails ?? Enumerable.Empty<AttackDetailDTO>())
                .Select(a => new AttackDetailResponse
                {
                    AttackName = a.AttackName,
                    AttackCost = a.AttackCost,
                    AttackDescription = a.AttackDescription,
                    AttackDamage = a.AttackDamage,
                }).ToList(),

                    AbilityDetails = (dto.AbilityDetails ?? Enumerable.Empty<AbilityDetailDTO>())
                .Select(b => new AbilityDetailResponse
                {
                    Text = b.Text,
                    Type = b.Type,
                    Name = b.Name
                }).ToList(),

                }).ToList();
                return responseList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new List<CardDetailResponse>();
            }
        }
        public async Task SaveCardsAsync(CancellationToken cancellationToken)
        {
            try
            {
                var setList = await _setRepository.GetAllSetsAsync();

                foreach (var set in setList)
                {
                    var externalCards = await GetPokemonCardBySetIdAsync(set.SetId);
                    if (externalCards == null || !externalCards.Any())
                        continue;

                    foreach (var c in externalCards)
                    {
                        try
                        {
                            // ---------------- CardImage ----------------
                            var cardImage = new CardImage
                            {
                                Small = c.Images?.Small,
                                Large = c.Images?.Large
                            };
                            cardImage = await _cardImageRepository.SaveImageCardAsync(cardImage, cancellationToken);

                            // ---------------- Legalities ----------------
                            var cardLegality = new Legality
                            {
                                Expanded = c.Legalities?.Expanded,
                                Standard = c.Legalities?.Standard,
                                Unlimited = c.Legalities?.Unlimited
                            };
                            cardLegality = await _legalityRepository.SaveLegalityAsync(cardLegality, cancellationToken);

                            // ---------------- Abilities ----------------
                            var cardAbilities = c.Abilities?.Select(a => new Ability
                            {
                                Name = a.Name,
                                Text = a.Text,
                                Type = a.Type
                            }).ToList() ?? new List<Ability>();

                            // ---------------- Attacks ----------------
                            var cardAttacks = c.Attacks?.Select(at => new Attack
                            {
                                Name = at.Name,
                                CostJson = at.Cost != null ? JsonSerializer.Serialize(at.Cost, new JsonSerializerOptions { WriteIndented = true }) : null,
                                ConvertedEnergyCost = at.ConvertedEnergyCost.ToString(),
                                Damage = at.Damage,
                                Text = at.Text
                            }).ToList() ?? new List<Attack>();

                            // ---------------- Resistances ----------------
                            var cardResistances = c.Resistances?.Select(r => new Resistance
                            {
                                Type = r.Type,
                                Value = r.Value,
                            }).ToList() ?? new List<Resistance>();

                            // ---------------- Weaknesses ----------------
                            var cardWeaknesses = c.Weaknesses?.Select(w => new Weakness
                            {
                                Type = w.Type,
                                Value = w.Value
                            }).ToList() ?? new List<Weakness>();

                            // ---------------- CardMarket ----------------
                            CardMarket? cardMarket = null;

                            if (c.Cardmarket != null)
                            {
                                cardMarket = new CardMarket
                                {
                                    Url = c.Cardmarket.Url != null ? new Uri(c.Cardmarket.Url.ToString()) : null,
                                    UpdatedAt = c.Cardmarket.UpdatedAt,
                                    CardMarketPrices = new List<CardMarketPrice>()
                                };

                                if (c.Cardmarket.Prices != null) // ✅ null check
                                {
                                    cardMarket.CardMarketPrices.Add(new CardMarketPrice
                                    {
                                        AverageSellPrice = c.Cardmarket.Prices.AverageSellPrice,
                                        LowPrice = c.Cardmarket.Prices.LowPrice,
                                        TrendPrice = c.Cardmarket.Prices.TrendPrice,
                                        ReverseHoloLow = c.Cardmarket.Prices.ReverseHoloLow,
                                        ReverseHoloTrend = c.Cardmarket.Prices.ReverseHoloTrend,
                                        LowPriceExPlus = c.Cardmarket.Prices.LowPriceExPlus,
                                        AverageDay = c.Cardmarket.Prices.AverageDay,
                                        AverageWeek = c.Cardmarket.Prices.AverageWeek,
                                        AverageMonth = c.Cardmarket.Prices.AverageMonth,
                                        AverageDayReverseHolo = c.Cardmarket.Prices.AverageDayReverseHolo,
                                        AverageWeekReverseHolo = c.Cardmarket.Prices.AverageWeekReverseHolo,
                                        AverageMonthReverseHolo = c.Cardmarket.Prices.AverageMonthReverseHolo
                                    });
                                }
                            }

                            // ---------------- TCGPlayer ----------------
                            Models.TCGPlayer? cardTcgPlayer = null;
                            if (c.Tcgplayer != null)
                            {
                                var prices = new List<Models.Price>();
                                if (c.Tcgplayer?.Prices?.Holofoil != null)
                                    prices.Add(new Models.Price
                                    {
                                        Type = PriceType.Holofoil,
                                        Low = c.Tcgplayer.Prices.Holofoil.Low,
                                        Mid = c.Tcgplayer.Prices.Holofoil.Mid,
                                        High = c.Tcgplayer.Prices.Holofoil.High,
                                        Market = c.Tcgplayer.Prices.Holofoil.Market,
                                        DirectLow = c.Tcgplayer.Prices.Holofoil.DirectLow
                                    });
                                if (c.Tcgplayer?.Prices?.ReverseHolofoil != null)
                                    prices.Add(new Models.Price
                                    {
                                        Type = PriceType.ReverseHolofoil,
                                        Low = c.Tcgplayer.Prices.ReverseHolofoil.Low,
                                        Mid = c.Tcgplayer.Prices.ReverseHolofoil.Mid,
                                        High = c.Tcgplayer.Prices.ReverseHolofoil.High,
                                        Market = c.Tcgplayer.Prices.ReverseHolofoil.Market,
                                        DirectLow = c.Tcgplayer.Prices.ReverseHolofoil.DirectLow
                                    });

                                if (c.Tcgplayer?.Prices?.Normal != null)
                                    prices.Add(new Models.Price
                                    {
                                        Type = PriceType.Normal,
                                        Low = c.Tcgplayer.Prices.Normal.Low,
                                        Mid = c.Tcgplayer.Prices.Normal.Mid,
                                        High = c.Tcgplayer.Prices.Normal.High,
                                        Market = c.Tcgplayer.Prices.Normal.Market,
                                        DirectLow = c.Tcgplayer.Prices.Normal.DirectLow
                                    });
                                if (c.Tcgplayer?.Prices?.The1StEdition != null)
                                    prices.Add(new Models.Price
                                    {
                                        Type = PriceType.FirstEdition,
                                        Low = c.Tcgplayer.Prices.The1StEdition.Low,
                                        Mid = c.Tcgplayer.Prices.The1StEdition.Mid,
                                        High = c.Tcgplayer.Prices.The1StEdition.High,
                                        Market = c.Tcgplayer.Prices.The1StEdition.Market,
                                        DirectLow = c.Tcgplayer.Prices.The1StEdition.DirectLow
                                    });
                                if (c.Tcgplayer?.Prices?.The1StEditionHolofoil != null)
                                    prices.Add(new Models.Price
                                    {
                                        Type = PriceType.FirstEditionHolofoil,
                                        Low = c.Tcgplayer.Prices.The1StEditionHolofoil.Low,
                                        Mid = c.Tcgplayer.Prices.The1StEditionHolofoil.Mid,
                                        High = c.Tcgplayer.Prices.The1StEditionHolofoil.High,
                                        Market = c.Tcgplayer.Prices.The1StEditionHolofoil.Market,
                                        DirectLow = c.Tcgplayer.Prices.The1StEditionHolofoil.DirectLow
                                    });
                                if (c.Tcgplayer?.Prices?.Unlimited != null)
                                    prices.Add(new Models.Price
                                    {
                                        Type = PriceType.Unlimited,
                                        Low = c.Tcgplayer.Prices.Unlimited.Low,
                                        Mid = c.Tcgplayer.Prices.Unlimited.Mid,
                                        High = c.Tcgplayer.Prices.Unlimited.High,
                                        Market = c.Tcgplayer.Prices.Unlimited.Market,
                                        DirectLow = c.Tcgplayer.Prices.Unlimited.DirectLow
                                    });
                                if (c.Tcgplayer?.Prices?.UnlimitedHolofoil != null)
                                    prices.Add(new Models.Price
                                    {
                                        Type = PriceType.Unlimited,
                                        Low = c.Tcgplayer.Prices.UnlimitedHolofoil.Low,
                                        Mid = c.Tcgplayer.Prices.UnlimitedHolofoil.Mid,
                                        High = c.Tcgplayer.Prices.UnlimitedHolofoil.High,
                                        Market = c.Tcgplayer.Prices.UnlimitedHolofoil.Market,
                                        DirectLow = c.Tcgplayer.Prices.UnlimitedHolofoil.DirectLow
                                    });
                                prices = prices.Where(p => p != null).ToList();

                                cardTcgPlayer = new TCGPlayer
                                {
                                    Url = c.Tcgplayer.Url != null ? new Uri(c.Tcgplayer.Url.ToString()) : null,
                                    UpdatedAt = c.Tcgplayer.UpdatedAt,
                                    TCGPlayerPrices = new List<TCGPlayerPrice>
                            {
                                new TCGPlayerPrice { Prices = prices }
                            }
                                };
                            }

                            // ---------------- Carta principal ----------------
                            var card = new Models.Card
                            {
                                CardId = c.Id,
                                ExternalId = c.Id,
                                Name = c.Name,
                                SuperType = c.Supertype,
                                Level = c.Level,
                                Hp = c.Hp,
                                Types = c.Types?.FirstOrDefault(),
                                EvolvesFrom = c.EvolvesFrom,
                                EvolvesTo = c.EvolvesTo?.FirstOrDefault(),
                                Abilities = cardAbilities,
                                Attacks = cardAttacks,
                                RetreatCost = c.RetreatCost != null ? JsonSerializer.Serialize(c.RetreatCost, new JsonSerializerOptions { WriteIndented = true }) : null,
                                ConvertedRetreatCost = c.ConvertedRetreatCost,
                                SetId = c.Set?.Id,
                                Number = c.Number,
                                Artist = c.Artist,
                                Rarity = c.Rarity,
                                NationalPokedexNumbers = c.NationalPokedexNumbers != null ? JsonSerializer.Serialize(c.NationalPokedexNumbers, new JsonSerializerOptions { WriteIndented = true }) : null,
                                LegalitiesId = cardLegality.LegalityId,
                                CardImageId = cardImage.CardImageId,
                                CardMarket = cardMarket,
                                Tcgplayer = cardTcgPlayer,
                                FlavorText = c.FlavorText,
                                SubTypes = c.Subtypes?.FirstOrDefault(),
                                Weaknesses = cardWeaknesses,
                                Resistances = cardResistances,
                                Rules = c.Rules?.FirstOrDefault(),
                                RegulationMark = c.RegulationMark
                            };

                            await _cardRepository.SaveCardAsync(card, cancellationToken);
                        }
                        catch (Exception cardEx)
                        {
                            _logger.LogError($"Error saving card {c.Id}: {cardEx.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving cards: {ex.Message}");
            }
        }
        public async Task<PagedResult<CardDetailDTO>> SearchCardsAsync(string? name = null, string? setId = null, string? ptcgoCode = null,
                                                                       string? supertype = null, string? subtype = null, string? type = null,
                                                                       string? rarity = null, int page = 1, int pageSize = 55)
        {
            return await _cardRepository.SearchCardsAsync(name, setId, ptcgoCode, supertype, subtype, type, rarity, page, pageSize);
        }
        public async Task<CardDetailResponse> GetCardsByRarityAsync(string rarity, int page = 1, int pageSize = 55)
        {
            var result = await SearchCardsAsync(rarity: rarity, page: page, pageSize: pageSize);

            return new CardDetailResponse
            {
                Page = result.Page,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount,
                Cards = result.Items
            };
        }
        public async Task<CardDetailResponse> GetCardsByTypeAsync(string type, int page = 1, int pageSize = 55)
        {
            var result = await SearchCardsAsync(type: type, page: page, pageSize: pageSize);

            return new CardDetailResponse
            {
                Page = result.Page,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount,
                Cards = result.Items
            };
        }
        public async Task<CardDetailResponse> GetCardsBySupertypeAsync(string supertype, int page = 1, int pageSize = 55)
        {
            var result = await SearchCardsAsync(supertype: supertype, page: page, pageSize: pageSize);

            return new CardDetailResponse
            {
                Page = result.Page,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount,
                Cards = result.Items
            };
        }
        public async Task<CardDetailResponse> GetCardsBySubtypeAsync(string subtype, int page = 1, int pageSize = 55)
        {
            var result = await SearchCardsAsync(subtype: subtype, page: page, pageSize: pageSize);

            return new CardDetailResponse
            {
                Page = result.Page,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount,
                Cards = result.Items
            };
        }
        public async Task<CardDetailResponse> GetCardsBySetAsync(string setId, int page = 1, int pageSize = 55)
        {
            var result = await SearchCardsAsync(setId: setId, page: page, pageSize: pageSize);

            return new CardDetailResponse
            {
                Page = result.Page,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount,
                Cards = result.Items
            };
        }

        public async Task<CardDetailResponse> GetCardsByNameAsync(string name, int page = 1, int pageSize = 55)
        {
            var result = await SearchCardsAsync(name: name, page: page, pageSize: pageSize);

            return new CardDetailResponse
            {
                Page = result.Page,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount,
                Cards = result.Items
            };
        }

        public async Task<List<CardDetailResponse>> GetDistinctTypesAsync()
        { 
            var distinctTypes = await _cardRepository.GetDistinctTypesAsync();
            return distinctTypes.Select(type => new CardDetailResponse { Type = type }).ToList();
        }
        public async Task<List<CardDetailResponse>> GetDistinctRaritiesAsync()
        {
            var distinctRarities = await _cardRepository.GetDistinctRaritiesAsync();
            return distinctRarities.Select(rarity => new CardDetailResponse { Rarity = rarity }).ToList();
        }
        public async Task<List<CardDetailResponse>> GetDistinctSubtypesAsync()
        {
            var distinctSubtypes = await _cardRepository.GetDistinctSubtypesAsync();
            return distinctSubtypes.Select(subtypes => new CardDetailResponse { Subtype = subtypes }).ToList();
        }
        public async Task<List<CardDetailResponse>> GetDistinctSupertypesAsync()
        {
            var distinctSupertypes = await _cardRepository.GetDistinctSupertypesAsync();
            return distinctSupertypes.Select(supertypes => new CardDetailResponse { Supertype = supertypes }).ToList();
        }

        public async Task<List<PokemonCard>> GetPokemonCardBySetIdAsync(string idPokemonSet)
        {
            var apiKey = "9e6b5ba1-0b91-46de-89fc-740efcccfb40";

            // Definimos la política de reintento
            var retryPolicy = Policy
                .Handle<HttpRequestException>() // por si hay problemas de red
                .OrResult<ApiResourceList<PokemonCard>>(r => r == null) // si la respuesta es nula                
                .WaitAndRetryAsync(
                    retryCount: 15,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)), // 2s, 4s, 8s
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        Console.WriteLine($"⚠️ Reintentando (intento {retryAttempt}) después de {timespan.TotalSeconds}s...");
                    });

            using var client = new PokemonApiClient(apiKey);

            var filter = PokemonFilterBuilder.CreatePokemonFilter()
                .AddSetId(idPokemonSet);

            // Ejecutamos la llamada con la política de retry
            var cards = await retryPolicy.ExecuteAsync(async () =>
            {
                var result = await client.GetApiResourceAsync<PokemonCard>(filter);

                return result;
            });

            return cards?.Results ?? new List<PokemonCard>();
        }
    }
}