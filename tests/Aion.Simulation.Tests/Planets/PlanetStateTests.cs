using Aion.Simulation.Planets;

namespace Aion.Simulation.Tests.Planets;

public class PlanetStateTests
{
    [Fact]
    public void Constructor_PreservesInitialState()
    {
        var id = PlanetId.New();

        var planet = new PlanetState(
            id,
            "Earth",
            5.9722e24,
            6_371_000);

        Assert.Equal(id, planet.Id);
        Assert.Equal("Earth", planet.Name);
        Assert.Equal(5.9722e24, planet.MassKilograms);
        Assert.Equal(6_371_000, planet.MeanRadiusMeters);
    }

    [Fact]
    public void Constructor_RejectsEmptyIdentity()
    {
        Assert.Throws<ArgumentException>(
            () => new PlanetState(
                default,
                "Earth",
                5.9722e24,
                6_371_000));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_RejectsInvalidName(string name)
    {
        Assert.Throws<ArgumentException>(
            () => new PlanetState(
                PlanetId.New(),
                name,
                5.9722e24,
                6_371_000));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Constructor_RejectsInvalidMass(double massKilograms)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new PlanetState(
                PlanetId.New(),
                "Earth",
                massKilograms,
                6_371_000));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Constructor_RejectsInvalidMeanRadius(double meanRadiusMeters)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new PlanetState(
                PlanetId.New(),
                "Earth",
                5.9722e24,
                meanRadiusMeters));
    }
}
