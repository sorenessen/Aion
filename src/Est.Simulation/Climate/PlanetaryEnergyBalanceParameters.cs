namespace Est.Simulation.Climate;

public sealed record PlanetaryEnergyBalanceParameters
{
    public PlanetaryEnergyBalanceParameters(
        double stellarFluxWattsPerSquareMeter,
        double effectiveLongwaveEmissivity,
        double effectiveHeatCapacityJoulesPerSquareMeterKelvin,
        double iceFreeAlbedo,
        double iceAlbedo,
        double fullIceTemperatureKelvin,
        double iceFreeTemperatureKelvin,
        double iceResponseTimescaleSeconds)
    {
        if (!double.IsFinite(stellarFluxWattsPerSquareMeter) ||
            stellarFluxWattsPerSquareMeter < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(stellarFluxWattsPerSquareMeter));
        }

        if (!double.IsFinite(effectiveLongwaveEmissivity) ||
            effectiveLongwaveEmissivity < 0 ||
            effectiveLongwaveEmissivity > 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(effectiveLongwaveEmissivity));
        }

        if (!double.IsFinite(effectiveHeatCapacityJoulesPerSquareMeterKelvin) ||
            effectiveHeatCapacityJoulesPerSquareMeterKelvin <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(effectiveHeatCapacityJoulesPerSquareMeterKelvin));
        }

        if (!double.IsFinite(iceFreeAlbedo) ||
            iceFreeAlbedo < 0 ||
            iceFreeAlbedo > 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(iceFreeAlbedo));
        }

        if (!double.IsFinite(iceAlbedo) ||
            iceAlbedo < 0 ||
            iceAlbedo > 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(iceAlbedo));
        }

        if (iceAlbedo < iceFreeAlbedo)
        {
            throw new ArgumentException(
                "Ice albedo cannot be lower than ice-free albedo.",
                nameof(iceAlbedo));
        }

        if (!double.IsFinite(fullIceTemperatureKelvin) ||
            fullIceTemperatureKelvin < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(fullIceTemperatureKelvin));
        }

        if (!double.IsFinite(iceFreeTemperatureKelvin) ||
            iceFreeTemperatureKelvin < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(iceFreeTemperatureKelvin));
        }

        if (iceFreeTemperatureKelvin <= fullIceTemperatureKelvin)
        {
            throw new ArgumentException(
                "Ice-free temperature must exceed full-ice temperature.",
                nameof(iceFreeTemperatureKelvin));
        }

        if (!double.IsFinite(iceResponseTimescaleSeconds) ||
            iceResponseTimescaleSeconds <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(iceResponseTimescaleSeconds));
        }

        StellarFluxWattsPerSquareMeter =
            stellarFluxWattsPerSquareMeter;

        EffectiveLongwaveEmissivity =
            effectiveLongwaveEmissivity;

        EffectiveHeatCapacityJoulesPerSquareMeterKelvin =
            effectiveHeatCapacityJoulesPerSquareMeterKelvin;

        IceFreeAlbedo = iceFreeAlbedo;
        IceAlbedo = iceAlbedo;
        FullIceTemperatureKelvin = fullIceTemperatureKelvin;
        IceFreeTemperatureKelvin = iceFreeTemperatureKelvin;
        IceResponseTimescaleSeconds = iceResponseTimescaleSeconds;
    }

    public double StellarFluxWattsPerSquareMeter { get; }

    public double EffectiveLongwaveEmissivity { get; }

    public double EffectiveHeatCapacityJoulesPerSquareMeterKelvin { get; }

    public double IceFreeAlbedo { get; }

    public double IceAlbedo { get; }

    public double FullIceTemperatureKelvin { get; }

    public double IceFreeTemperatureKelvin { get; }

    public double IceResponseTimescaleSeconds { get; }
}
