using System.Text.Json.Serialization;

namespace Est.Api.Contracts;

public sealed record CreateSessionRequest(
    [property: JsonRequired]
    PlanetCreationRequest[] Planets);

public sealed record PlanetCreationRequest(
    [property: JsonRequired]
    string Name,
    [property: JsonRequired]
    double MassKilograms,
    [property: JsonRequired]
    double MeanRadiusMeters,
    [property: JsonRequired]
    PlanetEnvironmentCreationRequest Environment,
    PlanetaryEnergyBalanceModelRequest? EnergyBalanceModel = null);

public sealed record PlanetaryEnergyBalanceModelRequest(
    [property: JsonRequired]
    double StellarFluxWattsPerSquareMeter,
    [property: JsonRequired]
    double EffectiveLongwaveEmissivity,
    [property: JsonRequired]
    double EffectiveHeatCapacityJoulesPerSquareMeterKelvin,
    [property: JsonRequired]
    double IceFreeAlbedo,
    [property: JsonRequired]
    double IceAlbedo,
    [property: JsonRequired]
    double FullIceTemperatureKelvin,
    [property: JsonRequired]
    double IceFreeTemperatureKelvin,
    [property: JsonRequired]
    double IceResponseTimescaleSeconds);

public sealed record PlanetEnvironmentCreationRequest(
    [property: JsonRequired]
    double MeanSurfaceTemperatureKelvin,
    [property: JsonRequired]
    double SurfaceWaterFraction,
    [property: JsonRequired]
    double IceCoverageFraction,
    [property: JsonRequired]
    AtmosphereCreationRequest Atmosphere);

public sealed record AtmosphereCreationRequest(
    [property: JsonRequired]
    double SurfacePressurePascals,
    [property: JsonRequired]
    IReadOnlyDictionary<string, double> CompositionByMoleFraction);
