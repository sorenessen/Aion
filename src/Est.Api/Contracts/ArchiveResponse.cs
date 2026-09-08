namespace Est.Api.Contracts;

public sealed record ArchiveResponse(
    Guid ArchiveId,
    Guid WorldId,
    Guid TimelineId);
