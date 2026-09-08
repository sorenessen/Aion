using System.Text.Json.Serialization;

namespace Est.Api.Contracts;

public sealed record ReplacePlanetEnvironmentRequest(
    [property: JsonRequired]
    double MeanSurfaceTemperatureKelvin,
    [property: JsonRequired]
    double SurfaceWaterFraction,
    [property: JsonRequired]
    double IceCoverageFraction,
    [property: JsonRequired]
    AtmosphereReplacementRequest Atmosphere);

public sealed record AtmosphereReplacementRequest(
    [property: JsonRequired]
    double SurfacePressurePascals,
    [property: JsonRequired]
    IReadOnlyDictionary<string, double> CompositionByMoleFraction);
