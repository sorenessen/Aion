using System.Collections.Immutable;
using Aion.Simulation.Worlds;

namespace Aion.Simulation.Causality;

public sealed record SimulationStepResult
{
    public SimulationStepResult(
        WorldState world,
        SimulationChange change)
        : this(
            world,
            change.ElapsedSeconds,
            [change])
    {
    }

    public SimulationStepResult(
        WorldState world,
        long elapsedSeconds,
        IEnumerable<SimulationChange> changes)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(changes);

        if (elapsedSeconds < 0)
            throw new ArgumentOutOfRangeException(nameof(elapsedSeconds));

        var changeArray = changes.ToImmutableArray();

        if (changeArray.Any(change => change is null))
            throw new ArgumentException(
                "Step changes cannot contain null entries.",
                nameof(changes));

        World = world;
        ElapsedSeconds = elapsedSeconds;
        Changes = changeArray;
    }

    public WorldState World { get; }

    public long ElapsedSeconds { get; }

    public ImmutableArray<SimulationChange> Changes { get; }

    public SimulationChange Change =>
        Changes.Length == 1
            ? Changes[0]
            : throw new InvalidOperationException(
                "This step does not contain exactly one change.");
}
