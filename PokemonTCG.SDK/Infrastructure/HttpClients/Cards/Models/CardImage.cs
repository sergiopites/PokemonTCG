namespace PokemonTCG.SDK.Infrastructure.HttpClients.Cards.Models;

using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

public class CardImage
{
    [Key]
    public int Id { get; set; }
    [JsonProperty("small")]
    public Uri? Small { get; set; }

    [JsonProperty("large")]
    public Uri? Large { get; set; }

}