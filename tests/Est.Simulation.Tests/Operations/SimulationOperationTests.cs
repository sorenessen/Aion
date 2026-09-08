using Est.Simulation.Operations;
using Est.Simulation.Planets;
using Est.Simulation.Time;
using Est.Simulation.Worlds;

namespace Est.Simulation.Tests.Operations;

public class SimulationOperationTests
{
    [Fact]
    public void AdvanceTimeOperation_AdvancesWorldWithoutChangingSource()
    {
        var world = new WorldState(
            WorldId.New(),
            new SimulationTime(100));

        var result = SimulationOperationExecutor.Apply(
            world,
            new AdvanceTimeOperation(60));

        Assert.Equal(100, world.CurrentTime.TotalSeconds);
        Assert.Equal(160, result.CurrentTime.TotalSeconds);
        Assert.Equal(world.Id, result.Id);
    }

    [Fact]
    public void ReplacePlanetEnvironmentOperation_ChangesOnlyTargetEnvironment()
    {
        var planet = CreateEarth();

        var world = new WorldState(
            WorldId.New(),
            SimulationTime.Zero,
            [planet]);

        var newEnvironment = new PlanetEnvironment(
            300,
            0.65,
            0.02,
            planet.Environment.Atmosphere);

        var result = SimulationOperationExecutor.Apply(
            world,
            new ReplacePlanetEnvironmentOperation(
                planet.Id,
                newEnvironment));

        var changedPlanet = Assert.Single(result.Planets);

        Assert.Equal(planet.Id, changedPlanet.Id);
        Assert.Equal(planet.Name, changedPlanet.Name);
        Assert.Equal(planet.MassKilograms, changedPlanet.MassKilograms);
        Assert.Equal(planet.MeanRadiusMeters, changedPlanet.MeanRadiusMeters);
        Assert.Same(newEnvironment, changedPlanet.Environment);

        Assert.Same(planet.Environment, world.Planets[0].Environment);
    }

    [Fact]
    public void ReplacePlanetEnvironmentOperation_RejectsUnknownPlanet()
    {
        var world = new WorldState(
            WorldId.New(),
            SimulationTime.Zero,
            [CreateEarth()]);

        var operation = new ReplacePlanetEnvironmentOperation(
            PlanetId.New(),
            new PlanetEnvironment(
                300,
                0.5,
                0,
                AtmosphereState.Vacuum));

        Assert.Throws<PlanetNotFoundException>(
            () => SimulationOperationExecutor.Apply(
                world,
                operation));
    }

    [Fact]
    public void SimulationOperationExecutor_RejectsNullOperation()
    {
        var world = new WorldState(
            WorldId.New(),
            SimulationTime.Zero);

        Assert.Throws<ArgumentNullException>(
            () => SimulationOperationExecutor.Apply(
                world,
                null!));
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
                0.03,
                new AtmosphereState(
                    101_325,
                    new Dictionary<string, double>
                    {
                        ["N2"] = 0.78,
                        ["O2"] = 0.21,
                        ["Ar"] = 0.01
                    })));
    }
}
