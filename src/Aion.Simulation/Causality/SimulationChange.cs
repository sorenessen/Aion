using Aion.Simulation.Operations;
using Aion.Simulation.Planets;

namespace Aion.Simulation.Causality;

public sealed record SimulationChange
{
    public SimulationChange(
        ISimulationOperation operation,
        string cause,
        string summary,
        PlanetId? affectedPlanetId,
        long elapsedSeconds,
        IReadOnlyDictionary<string, double>? metrics = null)
    {
        ArgumentNullException.ThrowIfNull(operation);

        if (string.IsNullOrWhiteSpace(cause))
        {
            throw new ArgumentException(
                "Change cause cannot be empty.",
                nameof(cause));
        }

        if (string.IsNullOrWhiteSpace(summary))
        {
            throw new ArgumentException(
                "Change summary cannot be empty.",
                nameof(summary));
        }

        if (elapsedSeconds < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(elapsedSeconds));
        }

        Operation = operation;
        Cause = cause;
        Summary = summary;
        AffectedPlanetId = affectedPlanetId;
        ElapsedSeconds = elapsedSeconds;

        Metrics = metrics is null
            ? new Dictionary<string, double>()
            : new Dictionary<string, double>(
                metrics,
                StringComparer.Ordinal);
    }

    public ISimulationOperation Operation { get; }

    public string Cause { get; }

    public string Summary { get; }

    public PlanetId? AffectedPlanetId { get; }

    public long ElapsedSeconds { get; }

    public IReadOnlyDictionary<string, double> Metrics { get; }
}
