using Microsoft.EntityFrameworkCore;
using PokemonTCG.API.Data;
using PokemonTCG.API.DTOs;
using PokemonTCG.API.Helpers;
using PokemonTCG.API.Models;

namespace PokemonTCG.API.Repositories
{
    public class CardRepository : ICardRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ICardRepository> _logger;

        public CardRepository(AppDbContext context, ILogger<ICardRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Card>> GetCardsByNumberAsync(string number)
        {
            try
            {
                return await _context.Cards.Where(c => c.Number == number).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving cards by number '{number}': {ex.Message}");
                return new List<Models.Card>();
            }
        }
        public async Task<List<CardDetailDTO>> GetCardsByCardIdAsync(string id)
        {
            try
            {
                var card = await _context.Cards
            .Where(c => c.CardId == id)
            .Select(c => new
            {
                Card = c,
                Set = c.Set,
                Image = c.CardImage,
                Attacks = c.Attacks,
                Abilities = c.Abilities,
                Resistances = c.Resistances,
                Weaknesses = c.Weaknesses

            })
            .Select(x => new CardDetailDTO
            {
                CardId = x.Card.CardId,
                ImageLarge = x.Image != null ? x.Image.Large : null,
                Name = x.Card.Name,
                SetName = x.Set.Name,
                SetId = x.Set.SetId,
                Rule = x.Card.Rules,
                Number = x.Card.Number,
                Subtype = x.Card.SubTypes,
                Type = x.Card.Types,
                Supertype = x.Card.SuperType,
                CardMarketUrl = x.Card.CardMarket.Url,
                TcgPlayerUrl = x.Card.Tcgplayer.Url,
                Rarity = x.Card.Rarity,
                Artist = x.Card.Artist,
                EvolvesFrom = x.Card.EvolvesFrom,
                EvolvesTo = x.Card.EvolvesTo,
                HP = x.Card.Hp,
                SetTotal = x.Set.PrintedTotal.ToString(),
                SetImage = x.Set.Images.Logo,
                SetSymbol = x.Set.Images.Symbol,
                ConvertTreatCost = x.Card.ConvertedRetreatCost,
                RetreatCost = x.Card.RetreatCost,
                ResistanceDetails = x.Resistances
                .Select(r => new ResistanceDetailDTO
                {
                    Type = r.Type,
                    Value = r.Value
                }).ToList(),
                WeaknessDetails = x.Weaknesses
                .Select(w => new WeaknessDetailDTO
                {
                    Type = w.Type,
                    Value = w.Value
                }).ToList(),
                AttackDetails = x.Attacks
                    .Select(a => new AttackDetailDTO
                    {
                        AttackName = a.Name,
                        AttackCost = a.CostJson,
                        AttackDescription = a.Text,
                        AttackDamage = a.Damage
                    })
                    .ToList(),
                AbilityDetails = x.Abilities
                    .Select(b => new AbilityDetailDTO
                    {
                        Text = b.Text,
                        Type = b.Type,
                        Name = b.Name

                    })
                    .ToList(),
            })
            .FirstOrDefaultAsync();
                return card != null ? new List<CardDetailDTO> { card } : new List<CardDetailDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving cards by ID '{id}': {ex.Message}");
                return new List<CardDetailDTO>();
            }
        }
        public async Task<List<Models.Card>> GetCardsBySuperTypeAsync(string supertype)
        {
            try
            {
                return await _context.Cards.Where(c => c.SuperType == supertype).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving cards by supertype '{supertype}': {ex.Message}");
                return new List<Models.Card>();
            }
        }

        public async Task<List<CardDetailDTO>> GetCardsBySet(string setId)
        {
            try
            {
                var result = await (from c in _context.Cards
                                    join s in _context.Sets on c.SetId equals s.SetId
                                    join si in _context.SetImages on s.ImagesId equals si.SetImageId into siJoin
                                    from si in siJoin.DefaultIfEmpty()
                                    join ci in _context.CardImages on c.CardImageId equals ci.CardImageId into ciJoin
                                    from ci in ciJoin.DefaultIfEmpty()
                                    where s.SetId == setId
                                    select new CardDetailDTO
                                    {
                                        CardId = c.CardId,
                                        ImageLarge = ci.Large,
                                        Name = c.Name,
                                        SetName = s.Name,
                                        SetId = s.SetId,
                                        SetPrintedTotal = s.Total.ToString(),
                                        Rule = c.Rules,
                                        Number = c.Number,
                                        SetImage = si.Logo,
                                        SetSymbol = si.Symbol,
                                        SetTotal = s.PrintedTotal.ToString(),
                                        SetSerie = s.Series,
                                        Ptcgocode = s.PtcgoCode,
                                        ReleaseDate = s.ReleaseDate,

                                    })
                                    .ToListAsync();

                var ordered = result
                    .OrderBy(x => int.TryParse(x.Number, out _) ? 0 : 1)
                    .ThenBy(x => int.TryParse(x.Number, out var n) ? n : int.MaxValue)
                    .ThenBy(x => x.Number)
                    .ToList();

                return ordered;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving all cards: {ex.Message}");
                return new List<CardDetailDTO>();
            }
        }
        public async Task<PagedResult<CardDetailDTO>> SearchCardsAsync(string? name = null, string? setId = null, string? ptcgoCode = null,
                                                                       string? supertype = null, string? subtype = null, string? type = null,
                                                                       string? rarity = null, int page = 1, int pageSize = 55, string? number = null)
        {
            var query = _context.Cards
                .AsNoTracking()
                .Include(c => c.Set)
                .Include(c => c.CardImage)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(c => EF.Functions.Like(c.Name, $"%{name}%"));

            if (!string.IsNullOrWhiteSpace(setId))
                query = query.Where(c => c.Set.SetId == setId);

            if (!string.IsNullOrWhiteSpace(ptcgoCode))
                query = query.Where(c => c.Set.PtcgoCode == ptcgoCode);

            if (!string.IsNullOrWhiteSpace(supertype))
                query = query.Where(c => c.SuperType == supertype);

            if (!string.IsNullOrWhiteSpace(subtype))
                query = query.Where(c => c.SubTypes.Contains(subtype));

            if (!string.IsNullOrWhiteSpace(type))
                query = query.Where(c => c.Types.Contains(type));

            if (!string.IsNullOrWhiteSpace(rarity))
                query = query.Where(c => c.Rarity == rarity);

            if (!string.IsNullOrWhiteSpace(number))
                query = query.Where(c => c.Number == number);

            var totalCount = await query.CountAsync();

            var items = await query
                //.OrderBy(c => c.Number)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CardDetailDTO
                {
                    CardId = c.CardId,
                    Name = c.Name,
                    SetName = c.Set.Name,
                    Ptcgocode = c.Set.PtcgoCode,
                    Supertype = c.SuperType,
                    Subtype = c.SubTypes,
                    SetId = c.Set.SetId,
                    Type = c.Types,
                    Rarity = c.Rarity,
                    ImageLarge = c.CardImage != null ? c.CardImage.Large : null
                })
                .ToListAsync();

            return new PagedResult<CardDetailDTO>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                Items = items
            };
        }
        public async Task<IEnumerable<string>> GetDistinctRaritiesAsync()
        {
            return await _context.Cards
                .Where(c => c.Rarity != null && c.Rarity != "")
                .Select(c => c.Rarity!)
                .Distinct()
                .OrderBy(r => r)
                .ToListAsync();
        }

        // 🔹 Obtener tipos únicos (Types puede ser lista separada por comas)
        public async Task<IEnumerable<string>> GetDistinctTypesAsync()
        {
            return await _context.Cards
                .Where(c => c.Types != null && c.Types != "")
                .Select(c => c.Types!)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();
        }

        // 🔹 Obtener supertypes únicos
        public async Task<IEnumerable<string>> GetDistinctSupertypesAsync()
        {
            return await _context.Cards
                .Where(c => c.SuperType != null && c.SuperType != "")
                .Select(c => c.SuperType!)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();
        }

        // 🔹 Obtener subtypes únicos (pueden venir como lista)
        public async Task<IEnumerable<string>> GetDistinctSubtypesAsync()
        {
            return (await _context.Cards
                .Where(c => !string.IsNullOrEmpty(c.SubTypes))
                .Select(c => c.SubTypes!)
                .ToListAsync())
                .SelectMany(subtypes => subtypes.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
                .Distinct()
                .OrderBy(s => s)
                .ToList();
        }
        public async Task<Card> SaveCardAsync(Card card, CancellationToken cancellationToken)
        {
            var existingCard = await _context.Cards
                .Include(c => c.Abilities)
                .Include(c => c.Attacks)
                .Include(c => c.Weaknesses)
                .Include(c => c.Resistances)
                .Include(c => c.CardMarket).ThenInclude(cm => cm.CardMarketPrices)
                .Include(c => c.Tcgplayer).ThenInclude(tp => tp.TCGPlayerPrices).ThenInclude(tpp => tpp.Prices)
                .FirstOrDefaultAsync(c => c.ExternalId == card.ExternalId, cancellationToken);

            // --- CREATE ---
            if (existingCard == null)
            {
                _context.Cards.Add(card);
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation($"Card: {card.ExternalId} was added successfully.");
                return card;
            }

            // --- UPDATE ---
            _context.Entry(existingCard).CurrentValues.SetValues(card);

            // ✅ Abilities
            MergeCollection(
                existingCard.Abilities,
                card.Abilities ?? new List<Ability>(),
                a => a.AbilityId,
                (db, inc) => db.Name == inc.Name,
                (db, inc) =>
                {
                    db.Name = inc.Name;
                    db.Text = inc.Text;
                    db.Type = inc.Type;
                },
                newItem => { newItem.CardId = existingCard.CardId; }
            );

            // ✅ Attacks
            MergeCollection(
                existingCard.Attacks,
                card.Attacks ?? new List<Attack>(),
                a => a.AttackId,
                (db, inc) => db.Name == inc.Name,
                (db, inc) =>
                {
                    db.Name = inc.Name;
                    db.CostJson = inc.CostJson;
                    db.ConvertedEnergyCost = inc.ConvertedEnergyCost;
                    db.Damage = inc.Damage;
                    db.Text = inc.Text;
                },
                newItem => { newItem.CardId = existingCard.CardId; }
            );

            // ✅ Resistances
            MergeCollection(
                existingCard.Resistances,
                card.Resistances ?? new List<Resistance>(),
                r => r.ResistanceId,
                (db, inc) => db.Type == inc.Type,
                (db, inc) =>
                {
                    db.Type = inc.Type;
                    db.Value = inc.Value;
                },
                newItem => { newItem.CardId = existingCard.CardId; }
            );

            // ✅ Weaknesses
            MergeCollection(
                existingCard.Weaknesses,
                card.Weaknesses ?? new List<Weakness>(),
                w => w.WeaknessId,
                (db, inc) => db.Type == inc.Type,
                (db, inc) =>
                {
                    db.Type = inc.Type;
                    db.Value = inc.Value;
                },
                newItem => { newItem.CardId = existingCard.CardId; }
            );

            // ✅ CardMarket (1:1 con Prices)
            if (card.CardMarket != null)
            {
                if (existingCard.CardMarket == null)
                {
                    existingCard.CardMarket = card.CardMarket;
                    existingCard.CardMarket.CardId = existingCard.CardId;
                }
                else
                {
                    existingCard.CardMarket.Url = card.CardMarket.Url;
                    existingCard.CardMarket.UpdatedAt = card.CardMarket.UpdatedAt;

                    MergeCollection(
                        existingCard.CardMarket.CardMarketPrices,
                        card.CardMarket.CardMarketPrices ?? new List<CardMarketPrice>(),
                        cmp => cmp.CardMarketPriceId,
                        (db, inc) => db.AverageSellPrice == inc.AverageSellPrice && db.LowPrice == inc.LowPrice,
                        (db, inc) =>
                        {
                            db.AverageSellPrice = inc.AverageSellPrice;
                            db.LowPrice = inc.LowPrice;
                            db.TrendPrice = inc.TrendPrice;
                        },
                        newItem => { newItem.CardMarketId = existingCard.CardMarket.CardMarketId; }
                    );
                }
            }

            // ✅ TCGPlayer (1:1 con Prices)
            if (card.Tcgplayer != null)
            {
                if (existingCard.Tcgplayer == null)
                {
                    existingCard.Tcgplayer = card.Tcgplayer;
                    existingCard.Tcgplayer.CardId = existingCard.CardId;
                }
                else
                {
                    existingCard.Tcgplayer.Url = card.Tcgplayer.Url;
                    existingCard.Tcgplayer.UpdatedAt = card.Tcgplayer.UpdatedAt;

                    MergeCollection(
                        existingCard.Tcgplayer.TCGPlayerPrices,
                        card.Tcgplayer.TCGPlayerPrices ?? new List<TCGPlayerPrice>(),
                        tpp => tpp.TCGPlayerPriceId,
                        (db, inc) => db.Prices?.Count == inc.Prices?.Count,
                        (db, inc) =>
                        {
                            MergeCollection(
                                db.Prices,
                                inc.Prices ?? new List<Price>(),
                                p => p.PriceId,
                                (dbp, incp) => dbp.Type == incp.Type,
                                (dbp, incp) =>
                                {
                                    dbp.Type = incp.Type;
                                    dbp.Low = incp.Low;
                                    dbp.Mid = incp.Mid;
                                    dbp.High = incp.High;
                                    dbp.Market = incp.Market;
                                    dbp.DirectLow = incp.DirectLow;
                                },
                                newPrice => { newPrice.TCGPlayerPriceId = db.TCGPlayerPriceId; }
                            );
                        },
                        newItem => { newItem.TCGPlayerId = existingCard.Tcgplayer.TCGPlayerId; }
                    );
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation($"Card: {existingCard.ExternalId} was updated successfully.");
            return existingCard;
        }

        private void MergeCollection<T, TKey>(
           ICollection<T> existingCollection,
           IEnumerable<T> incomingCollection,
           Func<T, TKey> keySelector,
           Func<T, T, bool>? fallbackMatch,
           Action<T, T> updateAction,
           Action<T>? onCreate = null)
           where T : class, new()
        {
            var incoming = incomingCollection?.ToList() ?? new List<T>();
            existingCollection ??= new List<T>();

            // Actualizar / Agregar
            foreach (var inc in incoming)
            {
                var key = keySelector(inc);
                T? match = default;

                // Si key es "significativo" (por ejemplo id != 0 o string no vacío), buscar por key
                if (key is int ik && ik != 0)
                    match = existingCollection.FirstOrDefault(e => object.Equals(keySelector(e), key));
                else if (key is long lk && lk != 0L)
                    match = existingCollection.FirstOrDefault(e => object.Equals(keySelector(e), key));
                else if (key is string sk && !string.IsNullOrWhiteSpace(sk))
                    match = existingCollection.FirstOrDefault(e => object.Equals(keySelector(e), key));
                else
                {
                    // fallback por predicate si se provee
                    if (fallbackMatch != null)
                        match = existingCollection.FirstOrDefault(e => fallbackMatch(e, inc));
                }

                if (match != null)
                {
                    // actualizar campos
                    updateAction(match, inc);
                }
                else
                {
                    // crear nuevo y ejecutar onCreate (p. ej. asignar FK)
                    var newItem = new T();
                    updateAction(newItem, inc);
                    onCreate?.Invoke(newItem);
                    existingCollection.Add(newItem);
                }
            }

            // Eliminar los que no están en incoming
            var toRemove = existingCollection
                .Where(db =>
                {
                    var dbKey = keySelector(db);
                    // si incoming tiene algún match por key
                    bool hasByKey = incoming.Any(inc =>
                    {
                        var incKey = keySelector(inc);
                        if (incKey is int ik && ik != 0 && dbKey is int dik) return ik == dik;
                        if (incKey is long lk && lk != 0L && dbKey is long dlk) return lk == dlk;
                        if (incKey is string sk && !string.IsNullOrWhiteSpace(sk) && dbKey is string dsk) return sk == dsk;
                        return false;
                    });

                    if (hasByKey) return false;

                    // fallback match
                    if (fallbackMatch != null)
                    {
                        return !incoming.Any(inc => fallbackMatch(db, inc));
                    }

                    // si no hay key ni fallback, y no se encontró por key => eliminar
                    return !hasByKey;
                })
                .ToList();

            foreach (var rem in toRemove)
                existingCollection.Remove(rem);
        }

    }
}