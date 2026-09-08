using Aion.Simulation.Timelines;

namespace Aion.Persistence.Archives;

public sealed record TimelineArchive(
    SimulationTimeline Timeline,
    TimelineArchiveProvenance Provenance);
