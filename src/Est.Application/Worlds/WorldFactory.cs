using Est.Simulation.Planets;
using Est.Simulation.Time;
using Est.Simulation.Worlds;

namespace Est.Application.Worlds;

public static class WorldFactory
{
    public static WorldState Create(
        WorldCreationSpecification specification)
    {
        ArgumentNullException.ThrowIfNull(specification);
        ArgumentNullException.ThrowIfNull(specification.Planets);

        var planets = specification.Planets
            .Select(CreatePlanet)
            .ToArray();

        return new WorldState(
            WorldId.New(),
            SimulationTime.Zero,
            planets);
    }

    private static PlanetState CreatePlanet(
        PlanetCreationSpecification specification)
    {
        ArgumentNullException.ThrowIfNull(specification);
        ArgumentNullException.ThrowIfNull(specification.Environment);
        ArgumentNullException.ThrowIfNull(
            specification.Environment.Atmosphere);

        var environment = specification.Environment;
        var atmosphere = environment.Atmosphere;

        return new PlanetState(
            PlanetId.New(),
            specification.Name,
            specification.MassKilograms,
            specification.MeanRadiusMeters,
            new PlanetEnvironment(
                environment.MeanSurfaceTemperatureKelvin,
                environment.SurfaceWaterFraction,
                environment.IceCoverageFraction,
                new AtmosphereState(
                    atmosphere.SurfacePressurePascals,
                    atmosphere.CompositionByMoleFraction)));
    }
}
