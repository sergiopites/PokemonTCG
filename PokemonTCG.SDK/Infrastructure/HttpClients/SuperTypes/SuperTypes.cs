namespace PokemonTCG.SDK.Infrastructure.HttpClients.SuperTypes;

using Newtonsoft.Json;
using System.Collections.Generic;

public class SuperTypes : ResourceBase
{
    [JsonProperty("data")]
    public List<string> SuperType { get; set; }

    internal new static string ApiEndpoint { get; } = "supertypes";

    public override string Id { get; set; }
}