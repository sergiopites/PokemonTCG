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
                    if (externalCards is not { Count: > 0 })
                        continue;

                    foreach (var c in externalCards)
                    {
                        try
                        {
                            var cardImage = await _cardImageRepository.SaveImageCardAsync(
                                MapCardImage(c), cancellationToken);

                            var cardLegality = await _legalityRepository.SaveLegalityAsync(
                                MapLegality(c), cancellationToken);

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
                                Abilities = MapAbilities(c),
                                Attacks = MapAttacks(c),
                                Resistances = MapResistances(c),
                                Weaknesses = MapWeaknesses(c),
                                RetreatCost = SerializeJson(c.RetreatCost),
                                ConvertedRetreatCost = c.ConvertedRetreatCost,
                                SetId = c.Set?.Id,
                                Number = c.Number,
                                Artist = c.Artist,
                                Rarity = c.Rarity,
                                NationalPokedexNumbers = SerializeJson(c.NationalPokedexNumbers),
                                LegalitiesId = cardLegality.LegalityId,
                                CardImageId = cardImage.CardImageId,
                                CardMarket = MapCardMarket(c),
                                Tcgplayer = MapTcgPlayer(c),
                                FlavorText = c.FlavorText,
                                SubTypes = c.Subtypes?.FirstOrDefault(),
                                Rules = c.Rules?.FirstOrDefault(),
                                RegulationMark = c.RegulationMark
                            };

                            await _cardRepository.SaveCardAsync(card, cancellationToken);
                        }
                        catch (Exception cardEx)
                        {
                            _logger.LogError("Error saving card {CardId}: {Message}", c.Id, cardEx.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error saving cards: {Message}", ex.Message);
            }
        }

        private static CardImage MapCardImage(PokemonCard c) => new()
        {
            Small = c.Images?.Small,
            Large = c.Images?.Large
        };

        private static Legality MapLegality(PokemonCard c) => new()
        {
            Expanded = c.Legalities?.Expanded,
            Standard = c.Legalities?.Standard,
            Unlimited = c.Legalities?.Unlimited
        };

        private static List<Ability> MapAbilities(PokemonCard c) =>
            c.Abilities?.Select(a => new Ability
            {
                Name = a.Name,
                Text = a.Text,
                Type = a.Type
            }).ToList() ?? [];

        private static List<Attack> MapAttacks(PokemonCard c) =>
            c.Attacks?.Select(at => new Attack
            {
                Name = at.Name,
                CostJson = SerializeJson(at.Cost),
                ConvertedEnergyCost = at.ConvertedEnergyCost.ToString(),
                Damage = at.Damage,
                Text = at.Text
            }).ToList() ?? [];

        private static List<Resistance> MapResistances(PokemonCard c) =>
            c.Resistances?.Select(r => new Resistance
            {
                Type = r.Type,
                Value = r.Value
            }).ToList() ?? [];

        private static List<Weakness> MapWeaknesses(PokemonCard c) =>
            c.Weaknesses?.Select(w => new Weakness
            {
                Type = w.Type,
                Value = w.Value
            }).ToList() ?? [];

        private static CardMarket? MapCardMarket(PokemonCard c)
        {
            if (c.Cardmarket is null)
                return null;

            var cardMarket = new CardMarket
            {
                Url = c.Cardmarket.Url != null ? new Uri(c.Cardmarket.Url.ToString()) : null,
                UpdatedAt = c.Cardmarket.UpdatedAt,
                CardMarketPrices = new List<CardMarketPrice>()
            };

            if (c.Cardmarket.Prices is { } p)
            {
                cardMarket.CardMarketPrices.Add(new CardMarketPrice
                {
                    AverageSellPrice = p.AverageSellPrice,
                    LowPrice = p.LowPrice,
                    TrendPrice = p.TrendPrice,
                    ReverseHoloLow = p.ReverseHoloLow,
                    ReverseHoloTrend = p.ReverseHoloTrend,
                    LowPriceExPlus = p.LowPriceExPlus,
                    AverageDay = p.AverageDay,
                    AverageWeek = p.AverageWeek,
                    AverageMonth = p.AverageMonth,
                    AverageDayReverseHolo = p.AverageDayReverseHolo,
                    AverageWeekReverseHolo = p.AverageWeekReverseHolo,
                    AverageMonthReverseHolo = p.AverageMonthReverseHolo
                });
            }

            return cardMarket;
        }

        private static Models.TCGPlayer? MapTcgPlayer(PokemonCard c)
        {
            if (c.Tcgplayer is null)
                return null;

            var prices = new List<Models.Price>();

            AddTcgPrice(prices, PriceType.Holofoil, c.Tcgplayer.Prices?.Holofoil);
            AddTcgPrice(prices, PriceType.ReverseHolofoil, c.Tcgplayer.Prices?.ReverseHolofoil);
            AddTcgPrice(prices, PriceType.Normal, c.Tcgplayer.Prices?.Normal);
            AddTcgPrice(prices, PriceType.FirstEdition, c.Tcgplayer.Prices?.The1StEdition);
            AddTcgPrice(prices, PriceType.FirstEditionHolofoil, c.Tcgplayer.Prices?.The1StEditionHolofoil);
            AddTcgPrice(prices, PriceType.Unlimited, c.Tcgplayer.Prices?.Unlimited);
            AddTcgPrice(prices, PriceType.Unlimited, c.Tcgplayer.Prices?.UnlimitedHolofoil);

            return new Models.TCGPlayer
            {
                Url = c.Tcgplayer.Url != null ? new Uri(c.Tcgplayer.Url.ToString()) : null,
                UpdatedAt = c.Tcgplayer.UpdatedAt,
                TCGPlayerPrices = [new TCGPlayerPrice { Prices = prices }]
            };
        }

        private static void AddTcgPrice(List<Models.Price> prices, PriceType type, dynamic? source)
        {
            if (source is null)
                return;

            prices.Add(new Models.Price
            {
                Type = type,
                Low = source.Low,
                Mid = source.Mid,
                High = source.High,
                Market = source.Market,
                DirectLow = source.DirectLow
            });
        }

        private static string? SerializeJson<T>(T? value) where T : class =>
            value is not null
                ? JsonSerializer.Serialize(value, new JsonSerializerOptions { WriteIndented = true })
                : null;

        public async Task<PagedResult<CardDetailDTO>> SearchCardsAsync(string? name = null, string? setId = null, string? ptcgoCode = null,
                                                                       string? supertype = null, string? subtype = null, string? type = null,
                                                                       string? rarity = null, int page = 1, int pageSize = 55, string? number = null)
        {
            return await _cardRepository.SearchCardsAsync(name, setId, ptcgoCode, supertype, subtype, type, rarity, page, pageSize, number);
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

            var retryPolicy = Policy
                .Handle<HttpRequestException>()
                .OrResult<ApiResourceList<PokemonCard>>(r => r == null)
                .WaitAndRetryAsync(
                    retryCount: 15,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        Console.WriteLine($"⚠️ Reintentando (intento {retryAttempt}) después de {timespan.TotalSeconds}s...");
                    });

            using var client = new PokemonApiClient(apiKey);

            var filter = PokemonFilterBuilder.CreatePokemonFilter()
                .AddSetId(idPokemonSet);

            var cards = await retryPolicy.ExecuteAsync(async () =>
            {
                var result = await client.GetApiResourceAsync<PokemonCard>(filter);
                return result;
            });

            return cards?.Results ?? new List<PokemonCard>();
        }
    }
}