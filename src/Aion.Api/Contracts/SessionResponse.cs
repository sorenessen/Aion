namespace Aion.Api.Contracts;

public sealed record SessionResponse(
    Guid SessionId,
    Guid WorldId,
    Guid TimelineId,
    long CurrentTimeSeconds,
    bool IsPaused,
    int PlanetCount,
    int EventCount,
    int CheckpointCount);
