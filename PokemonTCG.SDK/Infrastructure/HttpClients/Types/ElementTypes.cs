namespace PokemonTCG.SDK.Infrastructure.HttpClients.Types;

using Newtonsoft.Json;
using System.Collections.Generic;

public class ElementTypes : ResourceBase
{
    [JsonProperty("data")]
    public List<string> ElementType { get; set; }

    internal new static string ApiEndpoint { get; } = "types";

    public override string Id { get; set; }
}