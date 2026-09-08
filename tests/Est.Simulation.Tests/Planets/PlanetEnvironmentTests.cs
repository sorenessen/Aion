using Est.Simulation.Planets;

namespace Est.Simulation.Tests.Planets;

public class PlanetEnvironmentTests
{
    [Fact]
    public void Constructor_PreservesInitialState()
    {
        var atmosphere = AtmosphereState.Vacuum;

        var environment = new PlanetEnvironment(
            288.15,
            0.71,
            0.03,
            atmosphere);

        Assert.Equal(288.15, environment.MeanSurfaceTemperatureKelvin);
        Assert.Equal(0.71, environment.SurfaceWaterFraction);
        Assert.Equal(0.03, environment.IceCoverageFraction);
        Assert.Same(atmosphere, environment.Atmosphere);
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
                0.03,
                AtmosphereState.Vacuum));
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
                0.03,
                AtmosphereState.Vacuum));
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
                iceFraction,
                AtmosphereState.Vacuum));
    }

    [Fact]
    public void Constructor_AllowsIceCoverageGreaterThanWaterCoverage()
    {
        var environment = new PlanetEnvironment(
            288.15,
            0.20,
            0.30,
            AtmosphereState.Vacuum);

        Assert.Equal(0.20, environment.SurfaceWaterFraction);
        Assert.Equal(0.30, environment.IceCoverageFraction);
    }
    [Fact]
    public void Constructor_PreservesAtmosphere()
    {
        var atmosphere = new AtmosphereState(
            101_325,
            new Dictionary<string, double>
            {
                ["N2"] = 0.78,
                ["O2"] = 0.21,
                ["Ar"] = 0.01
            });

        var environment = new PlanetEnvironment(
            288.15,
            0.71,
            0.03,
            atmosphere);

        Assert.Equal(atmosphere, environment.Atmosphere);
    }

    [Fact]
    public void Constructor_RejectsNullAtmosphere()
    {
        Assert.Throws<ArgumentNullException>(
            () => new PlanetEnvironment(
                288.15,
                0.71,
                0.03,
                null!));
    }


}
