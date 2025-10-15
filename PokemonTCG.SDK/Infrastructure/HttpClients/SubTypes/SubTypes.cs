namespace PokemonTCG.SDK.Infrastructure.HttpClients.SubTypes;

using Newtonsoft.Json;
using System.Collections.Generic;

public class SubTypes : ResourceBase
{
    public override string Id { get; set; }

    internal new static string ApiEndpoint { get; } = "subtypes";

    [JsonProperty("data")]
    public List<string> SubType { get; set; }

}