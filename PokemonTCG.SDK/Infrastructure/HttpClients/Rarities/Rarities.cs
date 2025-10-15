namespace PokemonTCG.SDK.Infrastructure.HttpClients.Rarities;

using Newtonsoft.Json;
using System.Collections.Generic;

public class Rarities : ResourceBase
{
    [JsonProperty("data")]
    public List<string> Rarity { get; set; }

    internal new static string ApiEndpoint { get; } = "rarities";

    public override string Id { get; set; }
}