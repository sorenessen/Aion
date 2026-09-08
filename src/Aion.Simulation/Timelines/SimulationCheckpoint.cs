using Aion.Simulation.Time;
using Aion.Simulation.Worlds;

namespace Aion.Simulation.Timelines;

public sealed record SimulationCheckpoint
{
    public SimulationCheckpoint(
        Guid id,
        TimelineId timelineId,
        WorldState world)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "Checkpoint identity cannot be empty.",
                nameof(id));
        }

        if (timelineId.Value == Guid.Empty)
        {
            throw new ArgumentException(
                "Timeline identity cannot be empty.",
                nameof(timelineId));
        }

        ArgumentNullException.ThrowIfNull(world);

        Id = id;
        TimelineId = timelineId;
        World = world.Copy();
    }

    public Guid Id { get; }

    public TimelineId TimelineId { get; }

    public WorldState World { get; }

    public SimulationTime Time => World.CurrentTime;

    public static SimulationCheckpoint Create(
        TimelineId timelineId,
        WorldState world)
    {
        return new SimulationCheckpoint(
            Guid.NewGuid(),
            timelineId,
            world);
    }
}
