using Aion.Simulation.Definitions;
using Aion.Simulation.Timelines;

namespace Aion.Persistence.Archives;

public sealed record TimelineArchive
{
    public TimelineArchive(
        SimulationTimeline timeline,
        TimelineArchiveProvenance provenance)
        : this(
            timeline,
            SimulationDefinition.Empty,
            provenance)
    {
    }

    public TimelineArchive(
        SimulationTimeline timeline,
        SimulationDefinition definition,
        TimelineArchiveProvenance provenance)
    {
        ArgumentNullException.ThrowIfNull(timeline);
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(provenance);

        definition.ValidateFor(timeline.CurrentWorld);

        Timeline = timeline;
        Definition = definition;
        Provenance = provenance;
    }

    public SimulationTimeline Timeline { get; }

    public SimulationDefinition Definition { get; }

    public TimelineArchiveProvenance Provenance { get; }
}
