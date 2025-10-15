namespace PokemonTCG.SDK.Features.FilterBuilder.Trainer;

public static class TrainerFilterBuilder
{
    public static TrainerFilterCollection<string, string> CreateTrainerFilter()
    {
        return new TrainerFilterCollection<string, string>();
    }
}