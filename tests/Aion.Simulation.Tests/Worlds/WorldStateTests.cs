using Aion.Simulation.Planets;
using Aion.Simulation.Time;
using Aion.Simulation.Worlds;

namespace Aion.Simulation.Tests.Worlds;

public class WorldStateTests
{
    [Fact]
    public void Constructor_PreservesIdentityAndTime()
    {
        var id = WorldId.New();
        var time = new SimulationTime(500);

        var world = new WorldState(id, time);

        Assert.Equal(id, world.Id);
        Assert.Equal(time, world.CurrentTime);
    }

    [Fact]
    public void AdvanceBy_ReturnsNewWorldAtAdvancedTime()
    {
        var id = WorldId.New();
        var world = new WorldState(id, new SimulationTime(100));

        var advanced = world.AdvanceBy(60);

        Assert.Equal(id, advanced.Id);
        Assert.Equal(160, advanced.CurrentTime.TotalSeconds);
        Assert.Equal(100, world.CurrentTime.TotalSeconds);
    }

    [Fact]
    public void Constructor_RejectsEmptyWorldIdentity()
    {
        Assert.Throws<ArgumentException>(
            () => new WorldState(default, SimulationTime.Zero));
    }

    [Fact]
    public void AdvanceBy_NegativeDuration_Throws()
    {
        var world = new WorldState(
            WorldId.New(),
            SimulationTime.Zero);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => world.AdvanceBy(-1));
    }
    [Fact]
    public void Constructor_WithPlanet_PreservesPlanet()
    {
        var planet = CreateEarth();

        var world = new WorldState(
            WorldId.New(),
            SimulationTime.Zero,
            [planet]);

        Assert.Single(world.Planets);
        Assert.Equal(planet, world.Planets[0]);
    }

    [Fact]
    public void AddPlanet_ReturnsNewWorldContainingPlanet()
    {
        var world = new WorldState(
            WorldId.New(),
            SimulationTime.Zero);

        var planet = CreateEarth();

        var result = world.AddPlanet(planet);

        Assert.Empty(world.Planets);
        Assert.Single(result.Planets);
        Assert.Equal(planet, result.Planets[0]);
        Assert.Equal(world.Id, result.Id);
        Assert.Equal(world.CurrentTime, result.CurrentTime);
    }

    [Fact]
    public void AddPlanet_RejectsDuplicatePlanetIdentity()
    {
        var planet = CreateEarth();

        var world = new WorldState(
            WorldId.New(),
            SimulationTime.Zero,
            [planet]);

        Assert.Throws<InvalidOperationException>(
            () => world.AddPlanet(planet));
    }

    [Fact]
    public void Constructor_RejectsDuplicatePlanetIdentities()
    {
        var planet = CreateEarth();

        Assert.Throws<ArgumentException>(
            () => new WorldState(
                WorldId.New(),
                SimulationTime.Zero,
                [planet, planet]));
    }

    [Fact]
    public void ReplacePlanet_ReplacesMatchingPlanetWithoutChangingWorldIdentity()
    {
        var planet = CreateEarth();

        var world = new WorldState(
            WorldId.New(),
            new SimulationTime(100),
            [planet]);

        var replacement = new PlanetState(
            planet.Id,
            "Earth",
            5.9722e24,
            6_371_000,
            new PlanetEnvironment(
                289.15,
                0.71,
                0.03));

        var result = world.ReplacePlanet(replacement);

        Assert.Single(result.Planets);
        Assert.Equal(replacement, result.Planets[0]);
        Assert.Equal(world.Id, result.Id);
        Assert.Equal(world.CurrentTime, result.CurrentTime);
    }

    [Fact]
    public void ReplacePlanet_RejectsPlanetNotOwnedByWorld()
    {
        var world = new WorldState(
            WorldId.New(),
            SimulationTime.Zero,
            [CreateEarth()]);

        var otherPlanet = CreateEarth();

        Assert.Throws<InvalidOperationException>(
            () => world.ReplacePlanet(otherPlanet));
    }

    private static PlanetState CreateEarth()
    {
        return new PlanetState(
            PlanetId.New(),
            "Earth",
            5.9722e24,
            6_371_000,
            new PlanetEnvironment(
                288.15,
                0.71,
                0.03));
    }

}
