using Est.Simulation.Climate;
using Est.Simulation.Definitions;
using Est.Simulation.Planets;
using Est.Simulation.Time;
using Est.Simulation.Worlds;

namespace Est.Simulation.Tests.Definitions;

public class SimulationDefinitionTests
{
    [Fact]
    public void Empty_HasNoConfiguredModels()
    {
        Assert.Empty(
            SimulationDefinition.Empty
                .PlanetaryEnergyBalanceModels);
    }

    [Fact]
    public void Constructor_PreservesConfiguredModel()
    {
        var model = CreateModel();

        var definition =
            new SimulationDefinition([model]);

        var configured =
            Assert.Single(
                definition.PlanetaryEnergyBalanceModels);

        Assert.Same(model, configured);
    }

    [Fact]
    public void Constructor_RejectsDuplicateModelForPlanet()
    {
        var planetId = PlanetId.New();

        Assert.Throws<ArgumentException>(
            () => new SimulationDefinition(
            [
                CreateModel(planetId),
                CreateModel(planetId)
            ]));
    }

    [Fact]
    public void ModelDefinition_RejectsEmptyPlanetIdentity()
    {
        Assert.Throws<ArgumentException>(
            () => new PlanetaryEnergyBalanceModelDefinition(
                new PlanetId(Guid.Empty),
                CreateParameters()));
    }

    [Fact]
    public void ValidateFor_AcceptsModelForExistingPlanet()
    {
        var planetId = PlanetId.New();
        var definition = new SimulationDefinition(
            [CreateModel(planetId)]);

        var world = CreateWorld(planetId);

        definition.ValidateFor(world);
    }

    [Fact]
    public void ValidateFor_RejectsModelForUnknownPlanet()
    {
        var definition = new SimulationDefinition(
            [CreateModel(PlanetId.New())]);

        var world = CreateWorld(PlanetId.New());

        Assert.Throws<ArgumentException>(
            () => definition.ValidateFor(world));
    }

    [Fact]
    public void ValidateFor_EmptyDefinitionAcceptsEmptyWorld()
    {
        var world = new WorldState(
            WorldId.New(),
            SimulationTime.Zero);

        SimulationDefinition.Empty.ValidateFor(world);
    }

    private static WorldState CreateWorld(PlanetId planetId)
    {
        return new WorldState(
            WorldId.New(),
            SimulationTime.Zero,
            [
                new PlanetState(
                    planetId,
                    "Test Planet",
                    5.9722e24,
                    6_371_000,
                    new PlanetEnvironment(
                        288.15,
                        0.71,
                        0.03,
                        AtmosphereState.Vacuum))
            ]);
    }

    private static PlanetaryEnergyBalanceModelDefinition CreateModel(
        PlanetId? planetId = null)
    {
        return new PlanetaryEnergyBalanceModelDefinition(
            planetId ?? PlanetId.New(),
            CreateParameters());
    }

    private static PlanetaryEnergyBalanceParameters CreateParameters()
    {
        return new PlanetaryEnergyBalanceParameters(
            1361,
            0.61,
            1.0e8,
            0.30,
            0.60,
            263.15,
            273.15,
            31_536_000);
    }
}
