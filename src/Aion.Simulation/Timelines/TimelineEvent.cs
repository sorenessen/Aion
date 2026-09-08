using System.Collections.Immutable;
using Aion.Simulation.Causality;
using Aion.Simulation.Planets;
using Aion.Simulation.Time;

namespace Aion.Simulation.Timelines;

public sealed record TimelineEvent
{
    public TimelineEvent(
        Guid id,
        TimelineId timelineId,
        SimulationTime occurredAt,
        string cause,
        string summary,
        PlanetId? affectedPlanetId,
        long elapsedSeconds,
        IReadOnlyDictionary<string, double>? metrics = null)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "Timeline event identity cannot be empty.",
                nameof(id));
        }

        if (timelineId.Value == Guid.Empty)
        {
            throw new ArgumentException(
                "Timeline identity cannot be empty.",
                nameof(timelineId));
        }

        if (string.IsNullOrWhiteSpace(cause))
        {
            throw new ArgumentException(
                "Timeline event cause cannot be empty.",
                nameof(cause));
        }

        if (string.IsNullOrWhiteSpace(summary))
        {
            throw new ArgumentException(
                "Timeline event summary cannot be empty.",
                nameof(summary));
        }

        if (affectedPlanetId.HasValue &&
            affectedPlanetId.Value.Value == Guid.Empty)
        {
            throw new ArgumentException(
                "Affected planet identity cannot be empty.",
                nameof(affectedPlanetId));
        }

        if (elapsedSeconds < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(elapsedSeconds));
        }

        var metricBuilder =
            ImmutableDictionary.CreateBuilder<string, double>(
                StringComparer.Ordinal);

        if (metrics is not null)
        {
            foreach (var metric in metrics)
            {
                if (string.IsNullOrWhiteSpace(metric.Key))
                {
                    throw new ArgumentException(
                        "Timeline event metric names cannot be empty.",
                        nameof(metrics));
                }

                if (!double.IsFinite(metric.Value))
                {
                    throw new ArgumentException(
                        "Timeline event metrics must be finite.",
                        nameof(metrics));
                }

                metricBuilder.Add(
                    metric.Key,
                    metric.Value);
            }
        }

        Id = id;
        TimelineId = timelineId;
        OccurredAt = occurredAt;
        Cause = cause;
        Summary = summary;
        AffectedPlanetId = affectedPlanetId;
        ElapsedSeconds = elapsedSeconds;
        Metrics = metricBuilder.ToImmutable();
    }

    public Guid Id { get; }

    public TimelineId TimelineId { get; }

    public SimulationTime OccurredAt { get; }

    public string Cause { get; }

    public string Summary { get; }

    public PlanetId? AffectedPlanetId { get; }

    public long ElapsedSeconds { get; }

    public ImmutableDictionary<string, double> Metrics { get; }

    public static TimelineEvent FromChange(
        TimelineId timelineId,
        SimulationTime occurredAt,
        SimulationChange change)
    {
        ArgumentNullException.ThrowIfNull(change);

        return new TimelineEvent(
            Guid.NewGuid(),
            timelineId,
            occurredAt,
            change.Cause,
            change.Summary,
            change.AffectedPlanetId,
            change.ElapsedSeconds,
            change.Metrics);
    }
}
