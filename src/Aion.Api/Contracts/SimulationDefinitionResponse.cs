namespace Aion.Api.Contracts;

public sealed record SimulationDefinitionResponse(
    PlanetaryEnergyBalanceModelResponse[]
        PlanetaryEnergyBalanceModels);

public sealed record PlanetaryEnergyBalanceModelResponse(
    Guid PlanetId,
    double StellarFluxWattsPerSquareMeter,
    double EffectiveLongwaveEmissivity,
    double EffectiveHeatCapacityJoulesPerSquareMeterKelvin,
    double IceFreeAlbedo,
    double IceAlbedo,
    double FullIceTemperatureKelvin,
    double IceFreeTemperatureKelvin,
    double IceResponseTimescaleSeconds);
