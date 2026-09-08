using Aion.Simulation.Planets;

namespace Aion.Simulation.Tests.Planets;

public class AtmosphereStateTests
{
    [Fact]
    public void Constructor_PreservesPressureAndComposition()
    {
        var atmosphere = new AtmosphereState(
            101_325,
            new Dictionary<string, double>
            {
                ["N2"] = 0.78,
                ["O2"] = 0.21,
                ["Ar"] = 0.01
            });

        Assert.Equal(101_325, atmosphere.SurfacePressurePascals);
        Assert.Equal(0.78, atmosphere.CompositionByMoleFraction["N2"]);
        Assert.Equal(0.21, atmosphere.CompositionByMoleFraction["O2"]);
        Assert.Equal(0.01, atmosphere.CompositionByMoleFraction["Ar"]);
    }

    [Fact]
    public void Constructor_CopiesComposition()
    {
        var source = new Dictionary<string, double>
        {
            ["N2"] = 0.78,
            ["O2"] = 0.22
        };

        var atmosphere = new AtmosphereState(
            101_325,
            source);

        source["N2"] = 1.0;

        Assert.Equal(
            0.78,
            atmosphere.CompositionByMoleFraction["N2"]);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Constructor_RejectsInvalidPressure(double pressurePascals)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new AtmosphereState(
                pressurePascals,
                new Dictionary<string, double>()));
    }

    [Fact]
    public void Constructor_RejectsCompositionThatDoesNotSumToOne()
    {
        Assert.Throws<ArgumentException>(
            () => new AtmosphereState(
                101_325,
                new Dictionary<string, double>
                {
                    ["N2"] = 0.70,
                    ["O2"] = 0.20
                }));
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Constructor_RejectsInvalidMoleFraction(double fraction)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new AtmosphereState(
                101_325,
                new Dictionary<string, double>
                {
                    ["N2"] = fraction,
                    ["O2"] = 1.0
                }));
    }

    [Fact]
    public void Constructor_RejectsEmptySpeciesName()
    {
        Assert.Throws<ArgumentException>(
            () => new AtmosphereState(
                101_325,
                new Dictionary<string, double>
                {
                    [""] = 1.0
                }));
    }

    [Fact]
    public void Vacuum_HasZeroPressureAndNoComposition()
    {
        var atmosphere = AtmosphereState.Vacuum;

        Assert.Equal(0, atmosphere.SurfacePressurePascals);
        Assert.Empty(atmosphere.CompositionByMoleFraction);
    }

    [Fact]
    public void Constructor_RejectsCompositionForVacuum()
    {
        Assert.Throws<ArgumentException>(
            () => new AtmosphereState(
                0,
                new Dictionary<string, double>
                {
                    ["N2"] = 1.0
                }));
    }

    [Fact]
    public void Constructor_RejectsEmptyCompositionForNonZeroPressure()
    {
        Assert.Throws<ArgumentException>(
            () => new AtmosphereState(
                101_325,
                new Dictionary<string, double>()));
    }
}
