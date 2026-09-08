using Aion.Simulation.Causality;
using Aion.Simulation.Operations;
using Aion.Simulation.Planets;
using Aion.Simulation.Time;
using Aion.Simulation.Worlds;

namespace Aion.Simulation.Tests.Causality;

public class SimulationStepRunnerTests
{
    [Fact]
    public void Step_AppliesCausalOperationAndAdvancesTime()
    {
        var planet = CreateEarth();

        var world = new WorldState(
            WorldId.New(),
            new SimulationTime(100),
            [planet]);

        var replacementEnvironment =
            new PlanetEnvironment(
                290,
                0.71,
                0.02,
                planet.Environment.Atmosphere);

        var system = new FixedEnvironmentSystem(
            planet.Id,
            replacementEnvironment);

        var result = SimulationStepRunner.Step(
            world,
            60,
            system);

        Assert.Equal(
            160,
            result.CurrentTime.TotalSeconds);

        Assert.Same(
            replacementEnvironment,
            result.Planets[0].Environment);
    }

    [Fact]
    public void Step_DoesNotChangeSourceWorld()
    {
        var planet = CreateEarth();

        var world = new WorldState(
            WorldId.New(),
            new SimulationTime(100),
            [planet]);

        var system = new FixedEnvironmentSystem(
            planet.Id,
            new PlanetEnvironment(
                300,
                0.60,
                0,
                planet.Environment.Atmosphere));

        var result = SimulationStepRunner.Step(
            world,
            60,
            system);

        Assert.Equal(
            100,
            world.CurrentTime.TotalSeconds);

        Assert.Equal(
            288.15,
            world.Planets[0]
                .Environment
                .MeanSurfaceTemperatureKelvin);

        Assert.Equal(
            300,
            result.Planets[0]
                .Environment
                .MeanSurfaceTemperatureKelvin);
    }

    [Fact]
    public void Step_RejectsNegativeDuration()
    {
        var world = new WorldState(
            WorldId.New(),
            SimulationTime.Zero);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => SimulationStepRunner.Step(
                world,
                -1,
                new NoOpSystem()));
    }

    [Fact]
    public void Step_RejectsNullOperationFromCausalSystem()
    {
        var world = new WorldState(
            WorldId.New(),
            SimulationTime.Zero);

        Assert.Throws<InvalidOperationException>(
            () => SimulationStepRunner.Step(
                world,
                1,
                new InvalidSystem()));
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
                AtmosphereState.Vacuum));
    }

    private sealed class FixedEnvironmentSystem
        : ICausalSystem
    {
        private readonly PlanetId _planetId;
        private readonly PlanetEnvironment _environment;

        public FixedEnvironmentSystem(
            PlanetId planetId,
            PlanetEnvironment environment)
        {
            _planetId = planetId;
            _environment = environment;
        }

        public ISimulationOperation Evaluate(
            WorldState world,
            long elapsedSeconds)
        {
            return new ReplacePlanetEnvironmentOperation(
                _planetId,
                _environment);
        }
    }

    private sealed class NoOpSystem : ICausalSystem
    {
        public ISimulationOperation Evaluate(
            WorldState world,
            long elapsedSeconds)
        {
            return new AdvanceTimeOperation(0);
        }
    }

    private sealed class InvalidSystem : ICausalSystem
    {
        public ISimulationOperation Evaluate(
            WorldState world,
            long elapsedSeconds)
        {
            return null!;
        }
    }
}
