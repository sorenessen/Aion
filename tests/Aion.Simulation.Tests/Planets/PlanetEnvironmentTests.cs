using Aion.Simulation.Planets;

namespace Aion.Simulation.Tests.Planets;

public class PlanetEnvironmentTests
{
    [Fact]
    public void Constructor_PreservesInitialState()
    {
        var environment = new PlanetEnvironment(
            288.15,
            0.71,
            0.03);

        Assert.Equal(288.15, environment.MeanSurfaceTemperatureKelvin);
        Assert.Equal(0.71, environment.SurfaceWaterFraction);
        Assert.Equal(0.03, environment.IceCoverageFraction);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Constructor_RejectsInvalidTemperature(double temperatureKelvin)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new PlanetEnvironment(
                temperatureKelvin,
                0.71,
                0.03));
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Constructor_RejectsInvalidWaterFraction(double waterFraction)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new PlanetEnvironment(
                288.15,
                waterFraction,
                0.03));
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Constructor_RejectsInvalidIceFraction(double iceFraction)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new PlanetEnvironment(
                288.15,
                0.71,
                iceFraction));
    }

    [Fact]
    public void Constructor_AllowsIceCoverageGreaterThanWaterCoverage()
    {
        var environment = new PlanetEnvironment(
            288.15,
            0.20,
            0.30);

        Assert.Equal(0.20, environment.SurfaceWaterFraction);
        Assert.Equal(0.30, environment.IceCoverageFraction);
    }
}
