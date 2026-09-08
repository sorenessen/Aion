using Est.Simulation.Causality;
using Est.Simulation.Climate;
using Est.Simulation.Operations;
using Est.Simulation.Planets;
using Est.Simulation.Time;
using Est.Simulation.Worlds;

namespace Est.Simulation.Tests.Climate;

public class PlanetaryEnergyBalanceSystemTests
{
    private const double InitialTemperature = 288.15;
    private const double Emissivity = 0.61;
    private const double IceFreeAlbedo = 0.30;
    private const double IceAlbedo = 0.60;
    private const double HeatCapacity = 1.0e8;
    private const double OneDaySeconds = 86_400;
    private const double OneYearSeconds = 31_536_000;

    [Fact]
    public void Evaluate_IsDeterministicForSameInputs()
    {
        var planet = CreatePlanet(
            InitialTemperature,
            0);

        var world = CreateWorld(planet);

        var system = CreateSystem(
            planet.Id,
            EquilibriumStellarFlux());

        var first = system.Evaluate(
            world,
            86_400);

        var second = system.Evaluate(
            world,
            86_400);

        var firstOperation =
            Assert.IsType<ReplacePlanetEnvironmentOperation>(
                first.Operation);

        var secondOperation =
            Assert.IsType<ReplacePlanetEnvironmentOperation>(
                second.Operation);

        Assert.Equal(
            firstOperation.Environment,
            secondOperation.Environment);

        Assert.Equal(
            first.Metrics,
            second.Metrics);
    }

    [Fact]
    public void Step_AtRadiativeEquilibrium_RemainsStable()
    {
        var planet = CreatePlanet(
            InitialTemperature,
            0);

        var world = CreateWorld(planet);

        var system = CreateSystem(
            planet.Id,
            EquilibriumStellarFlux());

        var result = SimulationStepRunner.Step(
            world,
            (long)OneDaySeconds,
            system);

        Assert.Equal(
            InitialTemperature,
            result.World.Planets[0]
                .Environment
                .MeanSurfaceTemperatureKelvin,
            10);

        Assert.Equal(
            0,
            result.World.Planets[0]
                .Environment
                .IceCoverageFraction,
            10);
    }

    [Fact]
    public void Step_WithExcessIncomingEnergy_Warms()
    {
        var planet = CreatePlanet(
            InitialTemperature,
            0);

        var world = CreateWorld(planet);

        var system = CreateSystem(
            planet.Id,
            EquilibriumStellarFlux() * 1.10);

        var result = SimulationStepRunner.Step(
            world,
            (long)OneDaySeconds,
            system);

        Assert.True(
            result.World.Planets[0]
                .Environment
                .MeanSurfaceTemperatureKelvin
            > InitialTemperature);
    }

    [Fact]
    public void Step_WithEnergyDeficit_Cools()
    {
        var planet = CreatePlanet(
            InitialTemperature,
            0);

        var world = CreateWorld(planet);

        var system = CreateSystem(
            planet.Id,
            EquilibriumStellarFlux() * 0.90);

        var result = SimulationStepRunner.Step(
            world,
            (long)OneDaySeconds,
            system);

        Assert.True(
            result.World.Planets[0]
                .Environment
                .MeanSurfaceTemperatureKelvin
            < InitialTemperature);
    }

    [Fact]
    public void Step_WarmConditionsReduceIceCoverage()
    {
        var planet = CreatePlanet(
            280,
            0.50);

        var world = CreateWorld(planet);

        var system = CreateSystem(
            planet.Id,
            EquilibriumStellarFlux());

        var result = SimulationStepRunner.Step(
            world,
            (long)OneDaySeconds,
            system);

        Assert.True(
            result.World.Planets[0]
                .Environment
                .IceCoverageFraction
            < 0.50);
    }

    [Fact]
    public void Step_ColdConditionsIncreaseIceCoverage()
    {
        var planet = CreatePlanet(
            255,
            0.50);

        var world = CreateWorld(planet);

        var system = CreateSystem(
            planet.Id,
            EquilibriumStellarFlux());

        var result = SimulationStepRunner.Step(
            world,
            (long)OneDaySeconds,
            system);

        Assert.True(
            result.World.Planets[0]
                .Environment
                .IceCoverageFraction
            > 0.50);
    }

    [Fact]
    public void Evaluate_RejectsUnknownPlanet()
    {
        var world = CreateWorld(
            CreatePlanet(
                InitialTemperature,
                0));

        var system = CreateSystem(
            PlanetId.New(),
            EquilibriumStellarFlux());

        Assert.Throws<InvalidOperationException>(
            () => system.Evaluate(
                world,
                (long)OneDaySeconds));
    }

    private static PlanetaryEnergyBalanceSystem CreateSystem(
        PlanetId planetId,
        double stellarFlux)
    {
        return new PlanetaryEnergyBalanceSystem(
            planetId,
            new PlanetaryEnergyBalanceParameters(
                stellarFlux,
                Emissivity,
                HeatCapacity,
                IceFreeAlbedo,
                IceAlbedo,
                263.15,
                273.15,
                OneYearSeconds));
    }

    private static double EquilibriumStellarFlux()
    {
        var outgoing =
            Emissivity
            * PlanetaryEnergyBalanceSystem
                .StefanBoltzmannConstant
            * Math.Pow(
                InitialTemperature,
                4);

        return 4
            * outgoing
            / (1 - IceFreeAlbedo);
    }

    private static WorldState CreateWorld(
        PlanetState planet)
    {
        return new WorldState(
            WorldId.New(),
            SimulationTime.Zero,
            [planet]);
    }

    private static PlanetState CreatePlanet(
        double temperatureKelvin,
        double iceCoverageFraction)
    {
        return new PlanetState(
            PlanetId.New(),
            "Test Planet",
            5.9722e24,
            6_371_000,
            new PlanetEnvironment(
                temperatureKelvin,
                0.71,
                iceCoverageFraction,
                AtmosphereState.Vacuum));
    }
}
