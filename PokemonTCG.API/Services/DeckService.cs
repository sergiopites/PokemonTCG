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

            // proporciones (puedes ajustarlas)
            int pokemonCount = rnd.Next(16, 20);
            int trainerCount = rnd.Next(20, 26);
            int energyCount = TARGET - (pokemonCount + trainerCount);

            // obtener pools grandes
            var pokemon = await _cardRepository.SearchCardsAsync(supertype: "Pokémon", page: 1, pageSize: 2000);
            var trainer = await _cardRepository.SearchCardsAsync(supertype: "Trainer", page: 1, pageSize: 2000);
            var energy = await _cardRepository.SearchCardsAsync(supertype: "Energy", page: 1, pageSize: 1000);

            var pokemonPool = pokemon.Items?.ToList() ?? new List<CardDetailDTO>();
            var trainerPool = trainer.Items?.ToList() ?? new List<CardDetailDTO>();
            var energyPool = energy.Items?.ToList() ?? new List<CardDetailDTO>();

            if (!pokemonPool.Any()) throw new Exception("No hay cartas Pokémon disponibles.");

            // normalizadores sencillos
            string normalize(string s) => string.IsNullOrWhiteSpace(s) ? "" :
                Regex.Replace(s.ToLowerInvariant().Trim(), @"[\(\)\[\]""'`´’—–\-]+", "").Trim();

            // elegir tipo dominante
            var typeGroups = pokemonPool
                .Where(p => !string.IsNullOrWhiteSpace(p.Type))
                .GroupBy(p => p.Type)
                .OrderByDescending(g => g.Count())
                .ToList();

            var dominantType = typeGroups.Any()
                ? typeGroups[Math.Min(rnd.Next(3), typeGroups.Count - 1)].Key
                : pokemonPool.First().Type ?? "Colorless";

            var byType = pokemonPool.Where(p => p.Type == dominantType).ToList();
            if (!byType.Any()) byType = pokemonPool;

            // detectar power pokémon
            var powerPokemons = byType
                .Where(p =>
                    (p.Name?.IndexOf(" v", StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (p.Name?.IndexOf(" ex", StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (p.Name?.IndexOf(" vmax", StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (p.Name?.IndexOf(" vstar", StringComparison.OrdinalIgnoreCase) >= 0)
                )
                .GroupBy(p => normalize(p.Name))
                .Select(g => g.First())
                .ToList();

            var selectedPower = powerPokemons.OrderBy(_ => rnd.Next()).Take(Math.Min(3, Math.Max(1, powerPokemons.Count))).ToList();

            // helpers
            CardDetailDTO FindByNameInSet(List<CardDetailDTO> pool, string name, string setId)
            {
                if (string.IsNullOrWhiteSpace(name)) return null;
                var n = normalize(name);
                return pool.FirstOrDefault(p => normalize(p.Name) == n && p.SetId == setId)
                    ?? pool.FirstOrDefault(p => normalize(p.Name) == n);
            }

            List<CardDetailDTO> BuildFullLine(CardDetailDTO seed, List<CardDetailDTO> pool)
            {
                var line = new List<CardDetailDTO>();
                if (seed == null) return line;

                string seedName = seed.Name;
                string seedSet = seed.SetId;
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
                    if (line.Any(x => normalize(x.Name) == normalize(next.Name))) break;
                    walker = next;
                }

                return line.DistinctBy(x => normalize(x.Name)).ToList();
            }

            // Pokémon
            var finalPokemon = new List<CardDetailDTO>();
            foreach (var p in selectedPower)
            {
                if (finalPokemon.Count >= pokemonCount) break;
                if (finalPokemon.Count(x => normalize(x.Name) == normalize(p.Name)) < MAX_COPIES)
                    finalPokemon.Add(p);
            }

            var basicsCandidates = byType
                .Where(p => (p.Subtype ?? "").IndexOf("basic", StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderBy(_ => rnd.Next())
                .ToList();

            foreach (var baseCandidate in basicsCandidates)
            {
                if (finalPokemon.Count >= pokemonCount) break;
                var line = BuildFullLine(baseCandidate, byType);
                if (line.Count == 0) continue;

                foreach (var c in line)
                {
                    var currentCopies = finalPokemon.Count(x => normalize(x.Name) == normalize(c.Name));
                    if (currentCopies < MAX_COPIES && finalPokemon.Count < pokemonCount)
                        finalPokemon.Add(c);
                }
            }

            // rellenar pokémon si faltan
            var poolByName = byType.OrderBy(_ => rnd.Next()).ToList();
            int safety = 0;
            while (finalPokemon.Count < pokemonCount && safety++ < 10000)
            {
                var cand = poolByName[rnd.Next(poolByName.Count)];
                var curCopies = finalPokemon.Count(x => normalize(x.Name) == normalize(cand.Name));
                if (curCopies < MAX_COPIES)
                    finalPokemon.Add(cand);
            }

            // Trainers
            var finalTrainers = new List<CardDetailDTO>();
            var trainerShuffled = trainerPool.OrderBy(_ => rnd.Next()).ToList();
            foreach (var t in trainerShuffled)
            {
                if (finalTrainers.Count >= trainerCount) break;
                var curCopies = finalTrainers.Count(x => normalize(x.Name) == normalize(t.Name));
                if (curCopies < MAX_COPIES)
                    finalTrainers.Add(t);
            }

            // Energies
            var energyByType = energyPool.Where(e => (e.Name ?? "").IndexOf(dominantType, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            if (!energyByType.Any()) energyByType = energyPool;
            var chosenEnergy = energyByType.OrderBy(_ => rnd.Next()).FirstOrDefault();
            var finalEnergies = Enumerable.Repeat(chosenEnergy, Math.Max(0, energyCount)).ToList();

            // ensamblaje
            var combined = new List<CardDetailDTO>();
            combined.AddRange(finalPokemon);
            combined.AddRange(finalTrainers);
            combined.AddRange(finalEnergies);

            if (combined.Count < TARGET)
            {
                var need = TARGET - combined.Count;
                if (chosenEnergy != null)
                    combined.AddRange(Enumerable.Repeat(chosenEnergy, need));
            }

            combined = combined.Take(TARGET).ToList();

            var powerNames = selectedPower.Select(p => p.Name).Distinct().ToList();

            return new DeckDetailResponse
            {                
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
