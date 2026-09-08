using Aion.Application.Worlds;

namespace Aion.Application.Tests.Worlds;

public sealed class WorldFactoryTests
{
    [Fact]
    public void Create_EmptySpecification_CreatesEmptyWorld()
    {
        var world = WorldFactory.Create(
            new WorldCreationSpecification([]));

        Assert.NotEqual(Guid.Empty, world.Id.Value);
        Assert.Equal(0, world.CurrentTime.TotalSeconds);
        Assert.Empty(world.Planets);
    }

    [Fact]
    public void Create_PreservesPlanetaryInputs()
    {
        var world = WorldFactory.Create(
            new WorldCreationSpecification(
            [
                new PlanetCreationSpecification(
                    "Test Earth",
                    5.9722e24,
                    6_371_000,
                    new PlanetEnvironmentCreationSpecification(
                        288.15,
                        0.71,
                        0.10,
                        new AtmosphereCreationSpecification(
                            101_325,
                            new Dictionary<string, double>
                            {
                                ["N2"] = 0.78,
                                ["O2"] = 0.21,
                                ["Ar"] = 0.01
                            })))
            ]));

        var planet = Assert.Single(world.Planets);

        Assert.NotEqual(Guid.Empty, planet.Id.Value);
        Assert.Equal("Test Earth", planet.Name);
        Assert.Equal(5.9722e24, planet.MassKilograms);
        Assert.Equal(6_371_000, planet.MeanRadiusMeters);
        Assert.Equal(288.15,
            planet.Environment.MeanSurfaceTemperatureKelvin);
        Assert.Equal(0.71,
            planet.Environment.SurfaceWaterFraction);
        Assert.Equal(0.10,
            planet.Environment.IceCoverageFraction);
        Assert.Equal(101_325,
            planet.Environment.Atmosphere.SurfacePressurePascals);
        Assert.Equal(0.78,
            planet.Environment.Atmosphere
                .CompositionByMoleFraction["N2"]);
    }

    [Fact]
    public void Create_InvalidPlanet_RejectsSpecification()
    {
        var specification = new WorldCreationSpecification(
        [
            new PlanetCreationSpecification(
                "Impossible",
                -1,
                1,
                new PlanetEnvironmentCreationSpecification(
                    300,
                    0,
                    0,
                    new AtmosphereCreationSpecification(
                        0,
                        new Dictionary<string, double>())))
        ]);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => WorldFactory.Create(specification));
    }

    [Fact]
    public void Create_MissingNestedEnvironment_RejectsSpecification()
    {
        var specification = new WorldCreationSpecification(
        [
            new PlanetCreationSpecification(
                "Incomplete",
                1,
                1,
                null!)
        ]);

        Assert.Throws<ArgumentNullException>(
            () => WorldFactory.Create(specification));
    }

    [Fact]
    public void Create_AssignsIndependentWorldAndPlanetIdentities()
    {
        var specification = new WorldCreationSpecification(
        [
            new PlanetCreationSpecification(
                "Test",
                1,
                1,
                new PlanetEnvironmentCreationSpecification(
                    300,
                    0,
                    0,
                    new AtmosphereCreationSpecification(
                        0,
                        new Dictionary<string, double>())))
        ]);

        var first = WorldFactory.Create(specification);
        var second = WorldFactory.Create(specification);

        Assert.NotEqual(first.Id, second.Id);
        Assert.NotEqual(
            first.Planets[0].Id,
            second.Planets[0].Id);
    }
}
