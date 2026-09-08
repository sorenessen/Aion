using Est.Simulation.Causality;
using Est.Simulation.Operations;
using Est.Simulation.Planets;
using Est.Simulation.Time;
using Est.Simulation.Worlds;

namespace Est.Simulation.Tests.Causality;

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
            result.World.CurrentTime.TotalSeconds);

        Assert.Same(
            replacementEnvironment,
            result.World.Planets[0].Environment);

        Assert.Equal(
            "test-environment-change",
            result.Change.Cause);
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
            result.World.Planets[0]
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

    [Fact]
    public void Step_MultipleSystemsAdvanceTimeOnceAndPreserveOrder()
    {
        var planet = CreateEarth();
        var world = new WorldState(
            WorldId.New(),
            new SimulationTime(100),
            [planet]);

        var firstEnvironment = new PlanetEnvironment(
            290, 0.71, 0.02, planet.Environment.Atmosphere);
        var secondEnvironment = new PlanetEnvironment(
            300, 0.71, 0.01, planet.Environment.Atmosphere);

        var result = SimulationStepRunner.Step(
            world,
            60,
            new ICausalSystem[]
            {
                new FixedEnvironmentSystem(planet.Id, firstEnvironment),
                new FixedEnvironmentSystem(planet.Id, secondEnvironment)
            });

        Assert.Equal(160, result.World.CurrentTime.TotalSeconds);
        Assert.Equal(60, result.ElapsedSeconds);
        Assert.Equal(2, result.Changes.Length);
        Assert.Same(secondEnvironment, result.World.Planets[0].Environment);
        Assert.Equal(100, world.CurrentTime.TotalSeconds);
        Assert.Same(planet.Environment, world.Planets[0].Environment);
    }

    [Fact]
    public void Step_LaterSystemFailureDoesNotChangeSourceWorld()
    {
        var planet = CreateEarth();
        var world = new WorldState(
            WorldId.New(),
            SimulationTime.Zero,
            [planet]);

        var originalEnvironment = planet.Environment;

        Assert.Throws<InvalidOperationException>(
            () => SimulationStepRunner.Step(
                world,
                60,
                new ICausalSystem[]
                {
                    new FixedEnvironmentSystem(
                        planet.Id,
                        new PlanetEnvironment(
                            300, 0.71, 0,
                            originalEnvironment.Atmosphere)),
                    new InvalidSystem()
                }));

        Assert.Equal(SimulationTime.Zero, world.CurrentTime);
        Assert.Same(originalEnvironment, world.Planets[0].Environment);
    }

    [Fact]
    public void Step_EmptySystemCollectionAdvancesTimeOnce()
    {
        var world = new WorldState(
            WorldId.New(),
            new SimulationTime(100));

        var result = SimulationStepRunner.Step(
            world,
            60,
            Array.Empty<ICausalSystem>());

        Assert.Equal(160, result.World.CurrentTime.TotalSeconds);

        var change = Assert.Single(result.Changes);

        Assert.Equal(
            "Explicit time advancement",
            change.Cause);

        Assert.Equal(
            "Advanced simulation time by 60 seconds.",
            change.Summary);

        Assert.Equal(
            60,
            change.ElapsedSeconds);
    }

    [Fact]
    public void Step_RejectsCausalOperationThatAdvancesTime()
    {
        var world = new WorldState(
            WorldId.New(),
            SimulationTime.Zero);

        Assert.Throws<InvalidOperationException>(
            () => SimulationStepRunner.Step(
                world,
                60,
                new ICausalSystem[]
                {
                    new TimeAdvancingSystem()
                }));
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

        public SimulationChange Evaluate(
            WorldState world,
            long elapsedSeconds)
        {
            return new SimulationChange(
                new ReplacePlanetEnvironmentOperation(
                    _planetId,
                    _environment),
                "test-environment-change",
                "Test environment replacement.",
                _planetId,
                elapsedSeconds);
        }
    }

    private sealed class NoOpSystem : ICausalSystem
    {
        public SimulationChange Evaluate(
            WorldState world,
            long elapsedSeconds)
        {
            return new SimulationChange(
                new AdvanceTimeOperation(0),
                "test-no-op",
                "Test no-op.",
                null,
                elapsedSeconds);
        }
    }

    private sealed class InvalidSystem : ICausalSystem
    {
        public SimulationChange Evaluate(
            WorldState world,
            long elapsedSeconds)
        {
            return null!;
        }
    }
    private sealed class TimeAdvancingSystem : ICausalSystem
    {
        public SimulationChange Evaluate(
            WorldState world,
            long elapsedSeconds)
        {
            return new SimulationChange(
                new AdvanceTimeOperation(1),
                "invalid-time-change",
                "Invalid causal time advancement.",
                null,
                elapsedSeconds);
        }
    }


}
