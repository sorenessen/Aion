using Aion.Simulation.Time;
using Aion.Simulation.Timelines;
using Aion.Simulation.Worlds;

namespace Aion.Simulation.Tests.Timelines;

public class SimulationCheckpointTests
{
    [Fact]
    public void Create_CapturesWorldIdentityAndTime()
    {
        var world = new WorldState(
            WorldId.New(),
            new SimulationTime(500));

        var timelineId = TimelineId.New();

        var checkpoint =
            SimulationCheckpoint.Create(
                timelineId,
                world);

        Assert.NotEqual(Guid.Empty, checkpoint.Id);
        Assert.Equal(timelineId, checkpoint.TimelineId);
        Assert.Equal(world.Id, checkpoint.World.Id);
        Assert.Equal(500, checkpoint.Time.TotalSeconds);
    }

    [Fact]
    public void Create_PreservesStateWithoutSharingWorldInstance()
    {
        var world = new WorldState(
            WorldId.New(),
            new SimulationTime(100));

        var checkpoint =
            SimulationCheckpoint.Create(
                TimelineId.New(),
                world);

        Assert.NotSame(world, checkpoint.World);
        Assert.Equal(world.Id, checkpoint.World.Id);
        Assert.Equal(world.CurrentTime, checkpoint.World.CurrentTime);
    }

    [Fact]
    public void Constructor_RejectsEmptyCheckpointIdentity()
    {
        var world = new WorldState(
            WorldId.New(),
            SimulationTime.Zero);

        Assert.Throws<ArgumentException>(
            () => new SimulationCheckpoint(
                Guid.Empty,
                TimelineId.New(),
                world));
    }
}
