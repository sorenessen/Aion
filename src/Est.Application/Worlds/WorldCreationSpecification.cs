namespace Est.Application.Worlds;

public sealed record WorldCreationSpecification(
    IReadOnlyList<PlanetCreationSpecification> Planets);

public sealed record PlanetCreationSpecification(
    string Name,
    double MassKilograms,
    double MeanRadiusMeters,
    PlanetEnvironmentCreationSpecification Environment);

public sealed record PlanetEnvironmentCreationSpecification(
    double MeanSurfaceTemperatureKelvin,
    double SurfaceWaterFraction,
    double IceCoverageFraction,
    AtmosphereCreationSpecification Atmosphere);

public sealed record AtmosphereCreationSpecification(
    double SurfacePressurePascals,
    IReadOnlyDictionary<string, double> CompositionByMoleFraction);
