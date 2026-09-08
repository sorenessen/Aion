using Est.Simulation.Planets;
using Est.Simulation.Worlds;

namespace Est.Simulation.Operations;

public sealed record ReplacePlanetEnvironmentOperation
    : ISimulationOperation
{
    public ReplacePlanetEnvironmentOperation(
        PlanetId planetId,
        PlanetEnvironment environment)
    {
        if (planetId.Value == Guid.Empty)
        {
            throw new ArgumentException(
                "Planet identity cannot be empty.",
                nameof(planetId));
        }

        ArgumentNullException.ThrowIfNull(environment);

        PlanetId = planetId;
        Environment = environment;
    }

    public PlanetId PlanetId { get; }

    public PlanetEnvironment Environment { get; }

    public WorldState Apply(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);

        var planet = world.Planets
            .FirstOrDefault(candidate => candidate.Id == PlanetId);

        if (planet is null)
        {
            throw new PlanetNotFoundException(PlanetId);
        }

        var replacement = new PlanetState(
            planet.Id,
            planet.Name,
            planet.MassKilograms,
            planet.MeanRadiusMeters,
            Environment);

        return world.ReplacePlanet(replacement);
    }
}
