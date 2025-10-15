namespace PokemonTCG.SDK.Infrastructure.HttpClients.CommonModels;

using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

public class Legalities
{
    [Key]
    [JsonProperty("Id")]
    public int Id { get; set; }
    [JsonProperty("unlimited")]
    public string Unlimited { get; set; }

    [JsonProperty("standard")]
    public string Standard { get; set; }

    [JsonProperty("expanded")]
    public string Expanded { get; set; }
}