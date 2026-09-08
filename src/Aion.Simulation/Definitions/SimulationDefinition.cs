using System.Collections.Immutable;
using Aion.Simulation.Worlds;

namespace Aion.Simulation.Definitions;

public sealed record SimulationDefinition
{
    public SimulationDefinition(
        IEnumerable<PlanetaryEnergyBalanceModelDefinition>? planetaryEnergyBalanceModels = null)
    {
        var models = planetaryEnergyBalanceModels?
            .ToImmutableArray()
            ?? ImmutableArray<PlanetaryEnergyBalanceModelDefinition>.Empty;

        if (models.Any(model => model is null))
        {
            throw new ArgumentException(
                "Simulation definition cannot contain null model definitions.",
                nameof(planetaryEnergyBalanceModels));
        }

        if (models
            .GroupBy(model => model.PlanetId)
            .Any(group => group.Count() > 1))
        {
            throw new ArgumentException(
                "A planet cannot have more than one planetary energy-balance model definition.",
                nameof(planetaryEnergyBalanceModels));
        }

        PlanetaryEnergyBalanceModels = models;
    }

    public ImmutableArray<PlanetaryEnergyBalanceModelDefinition>
        PlanetaryEnergyBalanceModels { get; }

    public static SimulationDefinition Empty { get; } = new();

    public void ValidateFor(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);

        var planetIds = world.Planets
            .Select(planet => planet.Id)
            .ToHashSet();

        foreach (var model in PlanetaryEnergyBalanceModels)
        {
            if (!planetIds.Contains(model.PlanetId))
            {
                throw new ArgumentException(
                    $"Energy-balance model targets planet '{model.PlanetId.Value}', which does not exist in the world.",
                    nameof(world));
            }
        }
    }
}
