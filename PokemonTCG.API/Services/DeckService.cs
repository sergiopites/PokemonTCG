using PokemonTCG.API.DTOs;
using PokemonTCG.API.Repositories;
using PokemonTCG.API.Request;
using PokemonTCG.API.Responses;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PokemonTCG.API.Services
{
    public class DeckService : IDeckService
    {
        public readonly ILogger<IDeckRepository> _logger;
        public readonly IDeckRepository _deckRepository;
        public readonly ICardRepository _cardRepository;
        public DeckService(ILogger<IDeckRepository> logger, IDeckRepository deckRepository, ICardRepository cardRepository)
        {
            _logger = logger;
            _deckRepository = deckRepository;
            _cardRepository = cardRepository;
        }

        public async Task<DeckRequest> SaveDeckAsync(DeckRequest deckRequest, CancellationToken cancellationToken = default)
        {
            var deckDTO = new DeckDetailDTO
            {

                DeckId = deckRequest.DeckId,
                Name = deckRequest.Name,
                Description = deckRequest.Description,
                Cards = deckRequest.Cards.Select(c => new DeckCardDTO
                {
                    CardId = c.CardId,
                    Quantity = c.Quantity
                }).ToList()
            };
            deckDTO = await _deckRepository.SaveDeckAsync(deckDTO, cancellationToken);

            var result = new DeckRequest
            {
                DeckId = deckDTO.DeckId,
                Name = deckDTO.Name,
                Description = deckDTO.Description,
                Cards = deckDTO.Cards.Select(c => new CardQuantityRequest
                {
                    CardId = c.CardId,
                    Quantity = c.Quantity
                }).ToList()
            };
            return result;
        }

        public async Task<DeckDetailResponse> GenerateAutoDeckAsync()
        {
            var rnd = new Random();
            const int TARGET = 60;
            const int MAX_COPIES = 4;

            // Pokemon TCG official rules:
            // - Exactly 60 cards
            // - Max 4 copies of any card with same name (except Basic Energy = unlimited)
            // - Must have at least 1 Basic Pokemon to start the game
            // - Evolution lines need their pre-evolutions in the deck
            // - Competitive ratios: ~15-20 Pokemon, ~25-30 Trainers, ~10-15 Energy

            int pokemonTarget = rnd.Next(15, 21);
            int trainerTarget = rnd.Next(25, 31);
            int energyTarget = TARGET - (pokemonTarget + trainerTarget);
            if (energyTarget < 8) { trainerTarget -= (8 - energyTarget); energyTarget = 8; }

            var pokemon = await _cardRepository.SearchCardsAsync(supertype: "Pokémon", page: 1, pageSize: 2000);
            var trainer = await _cardRepository.SearchCardsAsync(supertype: "Trainer", page: 1, pageSize: 2000);
            var energy = await _cardRepository.SearchCardsAsync(supertype: "Energy", page: 1, pageSize: 1000);

            var pokemonPool = pokemon.Items?.ToList() ?? new List<CardDetailDTO>();
            var trainerPool = trainer.Items?.ToList() ?? new List<CardDetailDTO>();
            var energyPool = energy.Items?.ToList() ?? new List<CardDetailDTO>();

            if (!pokemonPool.Any()) throw new Exception("No Pokémon cards available.");

            string Normalize(string s) => string.IsNullOrWhiteSpace(s) ? "" :
                Regex.Replace(s.ToLowerInvariant().Trim(), @"[\(\)\[\]""'`´'—–\-]+", "").Trim();

            bool IsBasic(CardDetailDTO c) =>
                (c.Subtype ?? "").IndexOf("basic", StringComparison.OrdinalIgnoreCase) >= 0;

            bool IsBasicEnergy(CardDetailDTO c) =>
                (c.Supertype ?? "").Equals("Energy", StringComparison.OrdinalIgnoreCase) &&
                ((c.Subtype ?? "").IndexOf("basic", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 string.IsNullOrWhiteSpace(c.Subtype));

            // Choose dominant type from types with enough cards
            var typeGroups = pokemonPool
                .Where(p => !string.IsNullOrWhiteSpace(p.Type))
                .GroupBy(p => p.Type)
                .Where(g => g.Count() >= 10)
                .OrderByDescending(g => g.Count())
                .ToList();

            var dominantType = typeGroups.Any()
                ? typeGroups[Math.Min(rnd.Next(Math.Min(3, typeGroups.Count)), typeGroups.Count - 1)].Key
                : pokemonPool.First().Type ?? "Colorless";

            var byType = pokemonPool.Where(p => p.Type == dominantType).ToList();
            if (byType.Count < 10) byType = pokemonPool;

            // Helper to find card by name in same set preferably
            CardDetailDTO FindByNameInSet(List<CardDetailDTO> pool, string name, string setId)
            {
                if (string.IsNullOrWhiteSpace(name)) return null;
                var n = Normalize(name);
                return pool.FirstOrDefault(p => Normalize(p.Name) == n && p.SetId == setId)
                    ?? pool.FirstOrDefault(p => Normalize(p.Name) == n);
            }

            // Build full evolution line starting from any member
            List<CardDetailDTO> BuildFullLine(CardDetailDTO seed, List<CardDetailDTO> pool)
            {
                var line = new List<CardDetailDTO>();
                if (seed == null) return line;

                var current = seed;
                while (current != null)
                {
                    var prevName = current.EvolvesFrom;
                    if (string.IsNullOrWhiteSpace(prevName)) break;
                    var prev = FindByNameInSet(pool, prevName, current.SetId);
                    if (prev == null) break;
                    current = prev;
                }

                var walker = current ?? seed;
                while (walker != null)
                {
                    line.Add(walker);
                    var nextNameRaw = walker.EvolvesTo;
                    if (string.IsNullOrWhiteSpace(nextNameRaw)) break;
                    var nextName = nextNameRaw.Split(new[] { ',', '/' }, StringSplitOptions.RemoveEmptyEntries)[0].Trim();
                    var next = FindByNameInSet(pool, nextName, walker.SetId);
                    if (next == null) break;
                    if (line.Any(x => Normalize(x.Name) == Normalize(next.Name))) break;
                    walker = next;
                }

                return line.DistinctBy(x => Normalize(x.Name)).ToList();
            }

            // Track copies by name
            var copyCount = new Dictionary<string, int>();
            int CountCopies(string name) => copyCount.TryGetValue(Normalize(name), out var c) ? c : 0;
            void AddCopy(string name) { var n = Normalize(name); copyCount[n] = CountCopies(name) + 1; }

            // === BUILD POKEMON SECTION ===
            var finalPokemon = new List<CardDetailDTO>();

            // Detect power Pokemon (V, ex, VMAX, VSTAR) - these are Basic Rule Box cards
            var powerPokemons = byType
                .Where(p => IsBasic(p) &&
                    ((p.Name?.IndexOf(" v", StringComparison.OrdinalIgnoreCase) >= 0) ||
                     (p.Name?.IndexOf(" ex", StringComparison.OrdinalIgnoreCase) >= 0) ||
                     (p.Name?.IndexOf(" vmax", StringComparison.OrdinalIgnoreCase) >= 0) ||
                     (p.Name?.IndexOf(" vstar", StringComparison.OrdinalIgnoreCase) >= 0)))
                .GroupBy(p => Normalize(p.Name))
                .Select(g => g.First())
                .OrderBy(_ => rnd.Next())
                .Take(3)
                .ToList();

            // Add power Pokemon (max 4 copies each)
            foreach (var p in powerPokemons)
            {
                int copiesToAdd = rnd.Next(2, 4);
                for (int i = 0; i < copiesToAdd && finalPokemon.Count < pokemonTarget; i++)
                {
                    if (CountCopies(p.Name) < MAX_COPIES)
                    {
                        finalPokemon.Add(p);
                        AddCopy(p.Name);
                    }
                }
            }

            // Build evolution lines from Basic Pokemon with proper ratios
            // Rule: More Basics than Stage 1, more Stage 1 than Stage 2
            var basicCandidates = byType
                .Where(p => IsBasic(p) &&
                    !string.IsNullOrWhiteSpace(p.EvolvesTo) &&
                    !powerPokemons.Any(pp => Normalize(pp.Name) == Normalize(p.Name)))
                .GroupBy(p => Normalize(p.Name))
                .Select(g => g.First())
                .OrderBy(_ => rnd.Next())
                .ToList();

            foreach (var basicCard in basicCandidates)
            {
                if (finalPokemon.Count >= pokemonTarget) break;

                var line = BuildFullLine(basicCard, byType);
                if (line.Count == 0 || !IsBasic(line[0])) continue;

                // Proper evolution ratios: e.g. 4 Basic, 3 Stage 1, 2 Stage 2
                for (int stage = 0; stage < line.Count; stage++)
                {
                    var card = line[stage];
                    int copies;
                    if (stage == 0) copies = Math.Min(rnd.Next(3, 5), MAX_COPIES);
                    else if (stage == 1) copies = Math.Min(rnd.Next(2, 4), MAX_COPIES);
                    else copies = Math.Min(rnd.Next(1, 3), MAX_COPIES);

                    for (int i = 0; i < copies && finalPokemon.Count < pokemonTarget; i++)
                    {
                        if (CountCopies(card.Name) < MAX_COPIES)
                        {
                            finalPokemon.Add(card);
                            AddCopy(card.Name);
                        }
                    }
                }
            }

            // Fill remaining Pokemon slots with standalone Basics (no evolution)
            var standaloneBasics = byType
                .Where(p => IsBasic(p) &&
                    string.IsNullOrWhiteSpace(p.EvolvesTo) &&
                    !finalPokemon.Any(fp => Normalize(fp.Name) == Normalize(p.Name)))
                .GroupBy(p => Normalize(p.Name))
                .Select(g => g.First())
                .OrderBy(_ => rnd.Next())
                .ToList();

            foreach (var basic in standaloneBasics)
            {
                if (finalPokemon.Count >= pokemonTarget) break;
                int copies = rnd.Next(1, 3);
                for (int i = 0; i < copies && finalPokemon.Count < pokemonTarget; i++)
                {
                    if (CountCopies(basic.Name) < MAX_COPIES)
                    {
                        finalPokemon.Add(basic);
                        AddCopy(basic.Name);
                    }
                }
            }

            // Safety fill from Basics only
            var anyBasics = byType.Where(p => IsBasic(p)).OrderBy(_ => rnd.Next()).ToList();
            if (!anyBasics.Any()) anyBasics = pokemonPool.Where(p => IsBasic(p)).OrderBy(_ => rnd.Next()).ToList();
            int safety = 0;
            while (finalPokemon.Count < pokemonTarget && anyBasics.Any() && safety++ < 5000)
            {
                var cand = anyBasics[rnd.Next(anyBasics.Count)];
                if (CountCopies(cand.Name) < MAX_COPIES)
                {
                    finalPokemon.Add(cand);
                    AddCopy(cand.Name);
                }
            }

            // RULE: Must have at least 1 Basic Pokemon
            if (!finalPokemon.Any(IsBasic))
            {
                var emergencyBasic = byType.FirstOrDefault(IsBasic)
                    ?? pokemonPool.FirstOrDefault(IsBasic);
                if (emergencyBasic != null)
                {
                    if (finalPokemon.Count >= pokemonTarget && finalPokemon.Count > 0)
                        finalPokemon[finalPokemon.Count - 1] = emergencyBasic;
                    else
                        finalPokemon.Add(emergencyBasic);
                    AddCopy(emergencyBasic.Name);
                }
            }

            // === BUILD TRAINER SECTION ===
            // Include variety: Supporters, Items, Stadiums, Tools
            var finalTrainers = new List<CardDetailDTO>();

            var supporters = trainerPool
                .Where(t => (t.Subtype ?? "").IndexOf("supporter", StringComparison.OrdinalIgnoreCase) >= 0)
                .GroupBy(t => Normalize(t.Name)).Select(g => g.First()).OrderBy(_ => rnd.Next()).ToList();

            var items = trainerPool
                .Where(t => (t.Subtype ?? "").IndexOf("item", StringComparison.OrdinalIgnoreCase) >= 0)
                .GroupBy(t => Normalize(t.Name)).Select(g => g.First()).OrderBy(_ => rnd.Next()).ToList();

            var stadiums = trainerPool
                .Where(t => (t.Subtype ?? "").IndexOf("stadium", StringComparison.OrdinalIgnoreCase) >= 0)
                .GroupBy(t => Normalize(t.Name)).Select(g => g.First()).OrderBy(_ => rnd.Next()).ToList();

            var tools = trainerPool
                .Where(t => (t.Subtype ?? "").IndexOf("tool", StringComparison.OrdinalIgnoreCase) >= 0)
                .GroupBy(t => Normalize(t.Name)).Select(g => g.First()).OrderBy(_ => rnd.Next()).ToList();

            int supporterCount = Math.Min(rnd.Next(8, 13), supporters.Count * MAX_COPIES);
            int itemCount = Math.Min(rnd.Next(10, 15), items.Count * MAX_COPIES);
            int stadiumCount = Math.Min(rnd.Next(2, 4), stadiums.Count * MAX_COPIES);
            int toolCount = Math.Min(rnd.Next(2, 4), tools.Count * MAX_COPIES);

            void AddTrainers(List<CardDetailDTO> pool, int target, List<CardDetailDTO> dest)
            {
                int idx = 0;
                int added = 0;
                while (added < target && pool.Count > 0)
                {
                    var card = pool[idx % pool.Count];
                    if (CountCopies(card.Name) < MAX_COPIES)
                    {
                        dest.Add(card);
                        AddCopy(card.Name);
                        added++;
                    }
                    idx++;
                    if (idx >= pool.Count * MAX_COPIES) break;
                }
            }

            AddTrainers(supporters, supporterCount, finalTrainers);
            AddTrainers(items, itemCount, finalTrainers);
            AddTrainers(stadiums, stadiumCount, finalTrainers);
            AddTrainers(tools, toolCount, finalTrainers);

            // Fill remaining trainer slots
            int trainerActualTarget = TARGET - finalPokemon.Count - energyTarget;
            if (trainerActualTarget < 0) trainerActualTarget = 0;

            var remainingTrainers = trainerPool.OrderBy(_ => rnd.Next()).ToList();
            safety = 0;
            while (finalTrainers.Count < trainerActualTarget && remainingTrainers.Any() && safety++ < 5000)
            {
                var cand = remainingTrainers[rnd.Next(remainingTrainers.Count)];
                if (CountCopies(cand.Name) < MAX_COPIES)
                {
                    finalTrainers.Add(cand);
                    AddCopy(cand.Name);
                }
            }

            // === BUILD ENERGY SECTION ===
            // Basic Energy: unlimited copies allowed per Pokemon TCG rules
            int actualEnergyCount = TARGET - finalPokemon.Count - finalTrainers.Count;
            if (actualEnergyCount < 0) actualEnergyCount = 0;

            var basicEnergyByType = energyPool
                .Where(e => IsBasicEnergy(e) &&
                    (e.Name ?? "").IndexOf(dominantType, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();

            if (!basicEnergyByType.Any())
                basicEnergyByType = energyPool.Where(IsBasicEnergy).ToList();

            CardDetailDTO primaryEnergy = basicEnergyByType.OrderBy(_ => rnd.Next()).FirstOrDefault();

            // Secondary energy for dual-type coverage
            var secondaryType = finalPokemon
                .Where(p => !string.IsNullOrWhiteSpace(p.Type) && p.Type != dominantType)
                .GroupBy(p => p.Type)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault();

            CardDetailDTO secondaryEnergy = null;
            if (secondaryType != null)
            {
                secondaryEnergy = energyPool
                    .Where(e => IsBasicEnergy(e) &&
                        (e.Name ?? "").IndexOf(secondaryType, StringComparison.OrdinalIgnoreCase) >= 0)
                    .OrderBy(_ => rnd.Next())
                    .FirstOrDefault();
            }

            var finalEnergies = new List<CardDetailDTO>();
            if (primaryEnergy != null)
            {
                int primaryCount = secondaryEnergy != null
                    ? (int)Math.Ceiling(actualEnergyCount * 0.7)
                    : actualEnergyCount;
                int secondaryCount = actualEnergyCount - primaryCount;

                finalEnergies.AddRange(Enumerable.Repeat(primaryEnergy, primaryCount));
                if (secondaryEnergy != null && secondaryCount > 0)
                    finalEnergies.AddRange(Enumerable.Repeat(secondaryEnergy, secondaryCount));
            }
            else
            {
                var anyEnergy = energyPool.OrderBy(_ => rnd.Next()).FirstOrDefault();
                if (anyEnergy != null)
                    finalEnergies.AddRange(Enumerable.Repeat(anyEnergy, actualEnergyCount));
            }

            // === ASSEMBLE FINAL DECK ===
            var combined = new List<CardDetailDTO>();
            combined.AddRange(finalPokemon);
            combined.AddRange(finalTrainers);
            combined.AddRange(finalEnergies);

            // Adjust to exactly 60
            if (combined.Count < TARGET)
            {
                var need = TARGET - combined.Count;
                var filler = primaryEnergy ?? energyPool.FirstOrDefault();
                if (filler != null)
                    combined.AddRange(Enumerable.Repeat(filler, need));
            }
            combined = combined.Take(TARGET).ToList();

            var powerNames = powerPokemons.Select(p => p.Name).Distinct().ToList();

            var dominantPokemonName = powerNames.Any()
                ? powerNames.First()
                : finalPokemon.FirstOrDefault()?.Name ?? "Unknown";

            return new DeckDetailResponse
            {
                DeckName = $"{dominantPokemonName} - {dominantType}",
                DominantType = dominantType,
                PowerPokémon = powerNames,
                Total = combined.Count,
                Pokémon = finalPokemon.Count,
                Trainers = finalTrainers.Count,
                Energy = finalEnergies.Count,
                Cards = combined.Select(c => new DeckDetailResponse.DeckAutoCardDTO
                {
                    CardId = c.CardId,
                    ImageLarge = c.ImageLarge?.ToString(),
                    Name = c.Name,
                    SetName = c.SetName,
                    SetId = c.SetId,
                    Rule = c.Rule,
                    Subtype = c.Subtype,
                    Type = c.Type,
                    Supertype = c.Supertype,
                    Number = c.Number,
                    Ptcgocode = c.Ptcgocode,
                    EvolvesFrom = c.EvolvesFrom,
                    EvolvesTo = c.EvolvesTo
                }).ToList()
            };

        }

    }
}
