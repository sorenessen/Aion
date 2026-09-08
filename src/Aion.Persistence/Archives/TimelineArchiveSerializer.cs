using System.Text.Json;
using Aion.Persistence.Snapshots;
using Aion.Simulation.Planets;
using Aion.Simulation.Time;
using Aion.Simulation.Timelines;

namespace Aion.Persistence.Archives;

public static class TimelineArchiveSerializer
{
    public const int CurrentSchemaVersion = 1;

    private static readonly JsonSerializerOptions SerializerOptions =
        new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            AllowDuplicateProperties = false,
            UnmappedMemberHandling =
                System.Text.Json.Serialization
                    .JsonUnmappedMemberHandling.Disallow,
            WriteIndented = true
        };

    public static string Serialize(
        SimulationTimeline timeline,
        TimelineArchiveProvenance provenance)
    {
        ArgumentNullException.ThrowIfNull(timeline);
        ArgumentNullException.ThrowIfNull(provenance);

        var archive = new TimelineArchiveSnapshot
        {
            SchemaVersion = CurrentSchemaVersion,
            Provenance = new ProvenanceSnapshot
            {
                Producer = provenance.Producer,
                ProducerVersion = provenance.ProducerVersion,
                Origin = provenance.Origin
            },
            TimelineId = timeline.Id.Value,
            ParentTimelineId =
                timeline.ParentTimelineId?.Value,
            ParentCheckpointId =
                timeline.ParentCheckpointId,
            CurrentWorld = ToWorldElement(
                timeline.CurrentWorld),
            Checkpoints = timeline.Checkpoints
                .Select(ToCheckpointSnapshot)
                .ToArray(),
            Events = timeline.Events
                .Select(ToEventSnapshot)
                .ToArray()
        };

        return JsonSerializer.Serialize(
            archive,
            SerializerOptions);
    }

    public static TimelineArchive Deserialize(
        string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new ArgumentException(
                "Timeline archive JSON cannot be empty.",
                nameof(json));
        }

        var archive =
            JsonSerializer.Deserialize<TimelineArchiveSnapshot>(
                json,
                SerializerOptions)
            ?? throw new JsonException(
                "Archive JSON did not contain a timeline.");

        if (archive.SchemaVersion != CurrentSchemaVersion)
        {
            throw new NotSupportedException(
                $"Timeline archive schema version {archive.SchemaVersion} is not supported.");
        }

        if (archive.Provenance is null)
        {
            throw new JsonException(
                "Archive provenance is required.");
        }

        if (archive.Checkpoints is null ||
            archive.Checkpoints.Length == 0)
        {
            throw new JsonException(
                "Archive checkpoints are required.");
        }

        if (archive.Events is null)
        {
            throw new JsonException(
                "Archive events collection is required.");
        }

        var timelineId =
            new TimelineId(
                archive.TimelineId);

        var checkpoints = archive.Checkpoints
            .Select(snapshot =>
                FromCheckpointSnapshot(
                    timelineId,
                    snapshot))
            .ToArray();

        var events = archive.Events
            .Select(snapshot =>
                FromEventSnapshot(
                    timelineId,
                    snapshot))
            .ToArray();

        var currentWorld =
            FromWorldElement(
                archive.CurrentWorld);

        var timeline =
            SimulationTimeline.Restore(
                timelineId,
                archive.ParentTimelineId.HasValue
                    ? new TimelineId(
                        archive.ParentTimelineId.Value)
                    : null,
                archive.ParentCheckpointId,
                checkpoints[0],
                currentWorld,
                checkpoints,
                events);

        var provenance =
            new TimelineArchiveProvenance(
                archive.Provenance.Producer
                    ?? throw new JsonException(
                        "Archive producer is required."),
                archive.Provenance.ProducerVersion
                    ?? throw new JsonException(
                        "Archive producer version is required."),
                archive.Provenance.Origin
                    ?? throw new JsonException(
                        "Archive origin is required."));

        return new TimelineArchive(
            timeline,
            provenance);
    }

    private static CheckpointSnapshot ToCheckpointSnapshot(
        SimulationCheckpoint checkpoint)
    {
        return new CheckpointSnapshot
        {
            CheckpointId = checkpoint.Id,
            World = ToWorldElement(
                checkpoint.World)
        };
    }

    private static SimulationCheckpoint
        FromCheckpointSnapshot(
            TimelineId timelineId,
            CheckpointSnapshot snapshot)
    {
        return new SimulationCheckpoint(
            snapshot.CheckpointId,
            timelineId,
            FromWorldElement(snapshot.World));
    }

    private static TimelineEventSnapshot ToEventSnapshot(
        TimelineEvent timelineEvent)
    {
        return new TimelineEventSnapshot
        {
            EventId = timelineEvent.Id,
            OccurredAtSeconds =
                timelineEvent.OccurredAt.TotalSeconds,
            Cause = timelineEvent.Cause,
            Summary = timelineEvent.Summary,
            AffectedPlanetId =
                timelineEvent.AffectedPlanetId?.Value,
            ElapsedSeconds =
                timelineEvent.ElapsedSeconds,
            Metrics = timelineEvent.Metrics
                .ToDictionary(
                    pair => pair.Key,
                    pair => pair.Value,
                    StringComparer.Ordinal)
        };
    }

    private static TimelineEvent FromEventSnapshot(
        TimelineId timelineId,
        TimelineEventSnapshot snapshot)
    {
        if (snapshot.Metrics is null)
        {
            throw new JsonException(
                "Timeline event metrics are required.");
        }

        return new TimelineEvent(
            snapshot.EventId,
            timelineId,
            new SimulationTime(
                snapshot.OccurredAtSeconds),
            snapshot.Cause
                ?? throw new JsonException(
                    "Timeline event cause is required."),
            snapshot.Summary
                ?? throw new JsonException(
                    "Timeline event summary is required."),
            snapshot.AffectedPlanetId.HasValue
                ? new PlanetId(
                    snapshot.AffectedPlanetId.Value)
                : null,
            snapshot.ElapsedSeconds,
            snapshot.Metrics);
    }

    private static JsonElement ToWorldElement(
        Aion.Simulation.Worlds.WorldState world)
    {
        using var document =
            JsonDocument.Parse(
                WorldSnapshotSerializer.Serialize(world));

        return document.RootElement.Clone();
    }

    private static Aion.Simulation.Worlds.WorldState
        FromWorldElement(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            throw new JsonException(
                "Archive world snapshot is required.");
        }

        return WorldSnapshotSerializer.Deserialize(
            element.GetRawText());
    }

    private sealed class TimelineArchiveSnapshot
    {
        public required int SchemaVersion { get; set; }

        public required ProvenanceSnapshot Provenance { get; set; }

        public required Guid TimelineId { get; set; }

        public Guid? ParentTimelineId { get; set; }

        public Guid? ParentCheckpointId { get; set; }

        public required JsonElement CurrentWorld { get; set; }

        public required CheckpointSnapshot[] Checkpoints { get; set; }

        public required TimelineEventSnapshot[] Events { get; set; }
    }

    private sealed class ProvenanceSnapshot
    {
        public required string Producer { get; set; }

        public required string ProducerVersion { get; set; }

        public required string Origin { get; set; }
    }

    private sealed class CheckpointSnapshot
    {
        public required Guid CheckpointId { get; set; }

        public required JsonElement World { get; set; }
    }

    private sealed class TimelineEventSnapshot
    {
        public required Guid EventId { get; set; }

        public required long OccurredAtSeconds { get; set; }

        public required string Cause { get; set; }

        public required string Summary { get; set; }

        public Guid? AffectedPlanetId { get; set; }

        public required long ElapsedSeconds { get; set; }

        public required Dictionary<string, double> Metrics { get; set; }
    }
}
