namespace Est.Api.Contracts;

public sealed record TimelineResponse(
    Guid TimelineId,
    Guid WorldId,
    Guid? ParentTimelineId,
    Guid? ParentCheckpointId,
    long CurrentTimeSeconds,
    CheckpointResponse[] Checkpoints,
    TimelineEventResponse[] Events);

public sealed record CheckpointResponse(
    Guid CheckpointId,
    long TimeSeconds);

public sealed record TimelineEventResponse(
    Guid EventId,
    long OccurredAtSeconds,
    string Cause,
    string Summary,
    Guid? AffectedPlanetId,
    long ElapsedSeconds,
    IReadOnlyDictionary<string, double> Metrics);
