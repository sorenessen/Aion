using System.Text.Json;
using Aion.Simulation.Planets;
using Aion.Simulation.Time;
using Aion.Simulation.Worlds;

namespace Aion.Persistence.Snapshots;

public static class WorldSnapshotSerializer
{
    public const int CurrentSchemaVersion = 1;

    private static readonly JsonSerializerOptions SerializerOptions =
        new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            AllowDuplicateProperties = false,
            UnmappedMemberHandling = System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow,
            WriteIndented = true
        };

    public static string Serialize(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);

        var snapshot = new WorldSnapshot
        {
            SchemaVersion = CurrentSchemaVersion,
            WorldId = world.Id.Value,
            CurrentTimeSeconds = world.CurrentTime.TotalSeconds,
            Planets = world.Planets
                .Select(ToSnapshot)
                .ToArray()
        };

        return JsonSerializer.Serialize(
            snapshot,
            SerializerOptions);
    }

    public static WorldState Deserialize(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new ArgumentException(
                "Snapshot JSON cannot be empty.",
                nameof(json));
        }

        var snapshot = JsonSerializer.Deserialize<WorldSnapshot>(
            json,
            SerializerOptions)
            ?? throw new JsonException(
                "Snapshot JSON did not contain a world.");

        if (snapshot.SchemaVersion != CurrentSchemaVersion)
        {
            throw new NotSupportedException(
                $"Snapshot schema version {snapshot.SchemaVersion} is not supported.");
        }

        if (snapshot.Planets is null)
        {
            throw new JsonException(
                "Snapshot planets collection is required.");
        }

        var planets = snapshot.Planets
            .Select(FromSnapshot)
            .ToArray();

        return new WorldState(
            new WorldId(snapshot.WorldId),
            new SimulationTime(snapshot.CurrentTimeSeconds),
            planets);
    }

    private static PlanetSnapshot ToSnapshot(PlanetState planet)
    {
        return new PlanetSnapshot
        {
            PlanetId = planet.Id.Value,
            Name = planet.Name,
            MassKilograms = planet.MassKilograms,
            MeanRadiusMeters = planet.MeanRadiusMeters,
            Environment = new PlanetEnvironmentSnapshot
            {
                MeanSurfaceTemperatureKelvin =
                    planet.Environment.MeanSurfaceTemperatureKelvin,
                SurfaceWaterFraction =
                    planet.Environment.SurfaceWaterFraction,
                IceCoverageFraction =
                    planet.Environment.IceCoverageFraction,
                Atmosphere = new AtmosphereSnapshot
                {
                    SurfacePressurePascals =
                        planet.Environment.Atmosphere.SurfacePressurePascals,
                    CompositionByMoleFraction =
                        planet.Environment.Atmosphere
                            .CompositionByMoleFraction
                            .ToDictionary(
                                pair => pair.Key,
                                pair => pair.Value,
                                StringComparer.Ordinal)
                }
            }
        };
    }

    private static PlanetState FromSnapshot(PlanetSnapshot snapshot)
    {
        if (snapshot.Environment is null)
        {
            throw new JsonException(
                "Planet environment is required.");
        }

        if (snapshot.Environment.Atmosphere is null)
        {
            throw new JsonException(
                "Planet atmosphere is required.");
        }

        if (snapshot.Environment.Atmosphere.CompositionByMoleFraction is null)
        {
            throw new JsonException(
                "Atmospheric composition is required.");
        }

        var atmosphere = new AtmosphereState(
            snapshot.Environment.Atmosphere.SurfacePressurePascals,
            snapshot.Environment.Atmosphere.CompositionByMoleFraction);

        var environment = new PlanetEnvironment(
            snapshot.Environment.MeanSurfaceTemperatureKelvin,
            snapshot.Environment.SurfaceWaterFraction,
            snapshot.Environment.IceCoverageFraction,
            atmosphere);

        return new PlanetState(
            new PlanetId(snapshot.PlanetId),
            snapshot.Name
                ?? throw new JsonException(
                    "Planet name is required."),
            snapshot.MassKilograms,
            snapshot.MeanRadiusMeters,
            environment);
    }

    private sealed class WorldSnapshot
    {
        public required int SchemaVersion { get; set; }
        public required Guid WorldId { get; set; }
        public required long CurrentTimeSeconds { get; set; }
        public required PlanetSnapshot[] Planets { get; set; }
    }

    private sealed class PlanetSnapshot
    {
        public required Guid PlanetId { get; set; }
        public required string Name { get; set; }
        public required double MassKilograms { get; set; }
        public required double MeanRadiusMeters { get; set; }
        public required PlanetEnvironmentSnapshot Environment { get; set; }
    }

    private sealed class PlanetEnvironmentSnapshot
    {
        public required double MeanSurfaceTemperatureKelvin { get; set; }
        public required double SurfaceWaterFraction { get; set; }
        public required double IceCoverageFraction { get; set; }
        public required AtmosphereSnapshot Atmosphere { get; set; }
    }

    private sealed class AtmosphereSnapshot
    {
        public required double SurfacePressurePascals { get; set; }
        public required Dictionary<string, double> CompositionByMoleFraction
        {
            get;
            set;
        }
    }
}
