namespace Aion.Simulation.Planets;

public sealed record PlanetEnvironment
{
    public PlanetEnvironment(
        double meanSurfaceTemperatureKelvin,
        double surfaceWaterFraction,
        double iceCoverageFraction)
    {
        if (!double.IsFinite(meanSurfaceTemperatureKelvin) ||
            meanSurfaceTemperatureKelvin < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(meanSurfaceTemperatureKelvin),
                "Mean surface temperature must be a finite value at or above absolute zero.");
        }

        if (!double.IsFinite(surfaceWaterFraction) ||
            surfaceWaterFraction < 0 ||
            surfaceWaterFraction > 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(surfaceWaterFraction),
                "Surface water fraction must be between 0 and 1.");
        }

        if (!double.IsFinite(iceCoverageFraction) ||
            iceCoverageFraction < 0 ||
            iceCoverageFraction > 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(iceCoverageFraction),
                "Ice coverage fraction must be between 0 and 1.");
        }

        MeanSurfaceTemperatureKelvin = meanSurfaceTemperatureKelvin;
        SurfaceWaterFraction = surfaceWaterFraction;
        IceCoverageFraction = iceCoverageFraction;
    }

    public double MeanSurfaceTemperatureKelvin { get; }

    public double SurfaceWaterFraction { get; }

    public double IceCoverageFraction { get; }
}
