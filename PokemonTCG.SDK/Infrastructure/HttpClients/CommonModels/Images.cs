namespace PokemonTCG.SDK.Infrastructure.HttpClients.CommonModels;

using Newtonsoft.Json;
using System;

public class Images
{
    [JsonProperty("symbol")]
    public Uri Symbol { get; set; }

    [JsonProperty("logo")]
    public Uri Logo { get; set; }
}