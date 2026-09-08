using System.Text.Json.Serialization;

namespace Aion.Api.Contracts;

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
    PlanetEnvironmentCreationRequest Environment);

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
