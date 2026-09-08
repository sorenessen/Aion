using System.Collections.Immutable;
using Aion.Simulation.Planets;
using Aion.Simulation.Time;

namespace Aion.Simulation.Worlds;

public sealed record WorldState
{
    public WorldState(WorldId id, SimulationTime currentTime)
        : this(id, currentTime, [])
    {
    }

    public WorldState(
        WorldId id,
        SimulationTime currentTime,
        IEnumerable<PlanetState> planets)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException(
                "World identity cannot be empty.",
                nameof(id));
        }

        ArgumentNullException.ThrowIfNull(planets);

        var planetArray = planets.ToImmutableArray();

        if (planetArray.Any(planet => planet is null))
        {
            throw new ArgumentException(
                "World planets cannot contain null entries.",
                nameof(planets));
        }

        if (planetArray
            .GroupBy(planet => planet.Id)
            .Any(group => group.Count() > 1))
        {
            throw new ArgumentException(
                "World cannot contain duplicate planet identities.",
                nameof(planets));
        }

        Id = id;
        CurrentTime = currentTime;
        Planets = planetArray;
    }

    public WorldId Id { get; private init; }

    public SimulationTime CurrentTime { get; private init; }

    public ImmutableArray<PlanetState> Planets { get; private init; }

    public WorldState AdvanceBy(long seconds)
    {
        return this with
        {
            CurrentTime = CurrentTime.AdvanceBy(seconds)
        };
    }

    public WorldState Copy()
    {
        return new WorldState(
            Id,
            CurrentTime,
            Planets);
    }

    public WorldState Fork()
    {
        return new WorldState(
            WorldId.New(),
            CurrentTime,
            Planets);
    }

    public WorldState AddPlanet(PlanetState planet)
    {
        ArgumentNullException.ThrowIfNull(planet);

        if (Planets.Any(existing => existing.Id == planet.Id))
        {
            throw new InvalidOperationException(
                "A planet with this identity already exists in the world.");
        }

        return this with
        {
            Planets = Planets.Add(planet)
        };
    }

    public WorldState ReplacePlanet(PlanetState planet)
    {
        ArgumentNullException.ThrowIfNull(planet);

        for (var index = 0; index < Planets.Length; index++)
        {
            if (Planets[index].Id == planet.Id)
            {
                return this with
                {
                    Planets = Planets.SetItem(index, planet)
                };
            }
        }

        throw new InvalidOperationException(
            "The planet does not exist in this world.");
    }
}
