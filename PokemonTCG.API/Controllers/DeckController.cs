using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using PokemonTCG.API.DTOs;
using PokemonTCG.API.Request;
using PokemonTCG.API.Services;

namespace PokemonTCG.API.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class DeckController : ControllerBase
    {
        private readonly IDeckService _deckService;
        private readonly ICardService _cardService;
        private readonly ILogger<DeckController> _logger;
        public DeckController(IDeckService deckService, ICardService cardService, ILogger<DeckController> logger)
        {
            _deckService = deckService;
            _cardService = cardService;
            _logger = logger;
        }
        [HttpPost("create")]
        public async Task<IActionResult> SaveDeck(DeckRequest request, CancellationToken cancellationToken)
        {
            try
            {
                await _deckService.SaveDeckAsync(request, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in SaveDeck: {ex.Message}");
                return BadRequest(ex.Message);
            }
            return Ok();
        }
        [HttpGet("autodeck")]
        public async Task<IActionResult> AutoDeck()
        {
            var pokemon = await _cardService.SearchCardsAsync(null, null, "Pokémon", null, null, null, 1, 200);
            var trainer = await _cardService.SearchCardsAsync(null, null, "Trainer", null, null, null, 1, 200);
            var energy = await _cardService.SearchCardsAsync(null, null, "Energy", null, null, null, 1, 200);

            var rand = new Random();

            const int minPokemon = 15, maxPokemon = 20;
            const int minTrainer = 13, maxTrainer = 20;
            const int minEnergy = 15, maxEnergy = 19;
            const int targetTotal = 60;
            const int maxCopiesPerCard = 4;

            int pokemonCount = 0, trainerCount = 0, energyCount = 0;

            // Intentos aleatorios hasta encontrar una combinación válida que sume 60
            for (int i = 0; i < 1000; i++)
            {
                pokemonCount = rand.Next(minPokemon, maxPokemon + 1);
                trainerCount = rand.Next(minTrainer, maxTrainer + 1);
                energyCount = targetTotal - (pokemonCount + trainerCount);

                if (energyCount >= minEnergy && energyCount <= maxEnergy)
                    break;
            }

            // Si no se obtuvo una combinación válida, aplicar una estrategia determinista
            if (!(energyCount >= minEnergy && energyCount <= maxEnergy))
            {
                pokemonCount = minPokemon;
                trainerCount = minTrainer;
                energyCount = targetTotal - (pokemonCount + trainerCount);

                if (energyCount < minEnergy)
                {
                    int deficit = minEnergy - energyCount;
                    int reducibleTrainer = Math.Min(deficit, trainerCount - minTrainer);
                    trainerCount -= reducibleTrainer;
                    deficit -= reducibleTrainer;

                    int reduciblePokemon = Math.Min(deficit, pokemonCount - minPokemon);
                    pokemonCount -= reduciblePokemon;
                    deficit -= reduciblePokemon;

                    energyCount = targetTotal - (pokemonCount + trainerCount);
                }
                else if (energyCount > maxEnergy)
                {
                    int excess = energyCount - maxEnergy;
                    int increasableTrainer = Math.Min(excess, maxTrainer - trainerCount);
                    trainerCount += increasableTrainer;
                    excess -= increasableTrainer;

                    int increasablePokemon = Math.Min(excess, maxPokemon - pokemonCount);
                    pokemonCount += increasablePokemon;
                    excess -= increasablePokemon;

                    energyCount = targetTotal - (pokemonCount + trainerCount);
                }

                pokemonCount = Math.Clamp(pokemonCount, minPokemon, maxPokemon);
                trainerCount = Math.Clamp(trainerCount, minTrainer, maxTrainer);
                energyCount = targetTotal - (pokemonCount + trainerCount);
                energyCount = Math.Clamp(energyCount, minEnergy, maxEnergy);
            }

            // Nueva estrategia: iterar la pool barajada y asignar entre 1..maxCopies por carta hasta cubrir needed.
            static List<CardDetailDTO> PickWithMaxCopies(IList<CardDetailDTO> source, int needed, int maxCopies, Random rnd)
            {
                var result = new List<CardDetailDTO>();
                if (source == null || source.Count == 0 || needed <= 0) return result;

                var pool = source.OrderBy(x => rnd.Next()).ToList();
                int poolIndex = 0;
                // Intentamos asignar copias por carta de forma que con facilidad se generen hasta maxCopies
                while (result.Count < needed)
                {
                    var card = pool[poolIndex % pool.Count];
                    string cardKey = card.CardId ?? card.Name ?? (poolIndex % pool.Count).ToString();
                    int currentCopies = result.Count(c => (c.CardId ?? c.Name) == cardKey);
                    int canAdd = maxCopies - currentCopies;
                    if (canAdd > 0)
                    {
                        int remaining = needed - result.Count;
                        // Elegir entre 1 y canAdd copias (pero no más que lo que falta)
                        int toAdd = Math.Min(remaining, rnd.Next(1, canAdd + 1));
                        for (int k = 0; k < toAdd; k++) result.Add(card);
                    }

                    poolIndex++;

                    // Si hemos iterado muchas veces y no llenamos (pool muy pequeño), rellenamos respetando maxCopies
                    if (poolIndex > pool.Count * 10 && result.Count < needed)
                    {
                        foreach (var p in pool)
                        {
                            string pKey = p.CardId ?? p.Name ?? Guid.NewGuid().ToString();
                            while (result.Count(c => (c.CardId ?? c.Name) == pKey) < maxCopies && result.Count < needed)
                            {
                                result.Add(p);
                            }
                            if (result.Count >= needed) break;
                        }
                        // Si aún falta, repetir la primera carta
                        if (result.Count < needed)
                        {
                            var fallback = pool[0];
                            while (result.Count < needed) result.Add(fallback);
                        }
                        break;
                    }
                }

                return result;
            }

            var pokemonPool = pokemon.Items?.ToList() ?? new List<CardDetailDTO>();
            var trainerPool = trainer.Items?.ToList() ?? new List<CardDetailDTO>();
            var energyPool = energy.Items?.ToList() ?? new List<CardDetailDTO>();

            var selectedPokemon = PickWithMaxCopies(pokemonPool, pokemonCount, maxCopiesPerCard, rand);
            var selectedTrainer = PickWithMaxCopies(trainerPool, trainerCount, maxCopiesPerCard, rand);

            // ENERGY: todas iguales (elegir UNA carta energy y duplicarla energyCount veces)
            var selectedEnergy = new List<CardDetailDTO>();
            CardDetailDTO? chosenEnergy = null;
            if (energyCount > 0 && energyPool.Any())
            {
                chosenEnergy = energyPool.OrderBy(x => rand.Next()).First();
                selectedEnergy = Enumerable.Repeat(chosenEnergy, energyCount).ToList();
            }

            var selected = selectedPokemon
                .Concat(selectedTrainer)
                .Concat(selectedEnergy)
                .ToList();

            // Relleno final: si falta, primero añadir chosenEnergy (si existe), si no, intentar completar con Pokémon y Trainer respetando maxCopies.
            if (selected.Count < targetTotal)
            {
                int remaining = targetTotal - selected.Count;

                if (chosenEnergy != null)
                {
                    selected.AddRange(Enumerable.Repeat(chosenEnergy, remaining));
                    remaining = 0;
                }

                if (remaining > 0 && pokemonPool.Any())
                {
                    selected.AddRange(PickWithMaxCopies(pokemonPool, remaining, maxCopiesPerCard, rand));
                    remaining = targetTotal - selected.Count;
                }

                if (remaining > 0 && trainerPool.Any())
                {
                    selected.AddRange(PickWithMaxCopies(trainerPool, remaining, maxCopiesPerCard, rand));
                    remaining = targetTotal - selected.Count;
                }

                if (remaining > 0 && chosenEnergy == null && energyPool.Any())
                {
                    // elegir una energy y repetirla
                    chosenEnergy = energyPool.OrderBy(x => rand.Next()).First();
                    selected.AddRange(Enumerable.Repeat(chosenEnergy, remaining));
                }
            }

            if (selected.Count > targetTotal)
                selected = selected.Take(targetTotal).ToList();

            return Ok(selected);
        }

    }
}
