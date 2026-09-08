using Aion.Simulation.Time;
using Aion.Simulation.Timelines;
using Aion.Simulation.Worlds;

namespace Aion.Simulation.Tests.Timelines;

public class SimulationTimelineRestoreTests
{
    [Fact]
    public void Restore_PreservesExistingIdentitiesAndHistory()
    {
        var original = SimulationTimeline.Create(
            new WorldState(
                WorldId.New(),
                new SimulationTime(100)));

        var checkpointed = original.CreateCheckpoint();

        var restored = SimulationTimeline.Restore(
            checkpointed.Id,
            checkpointed.ParentTimelineId,
            checkpointed.ParentCheckpointId,
            checkpointed.InitialCheckpoint,
            checkpointed.CurrentWorld,
            checkpointed.Checkpoints,
            checkpointed.Events);

        Assert.Equal(checkpointed.Id, restored.Id);
        Assert.Equal(
            checkpointed.InitialCheckpoint.Id,
            restored.InitialCheckpoint.Id);

        Assert.Equal(
            checkpointed.Checkpoints.Select(x => x.Id),
            restored.Checkpoints.Select(x => x.Id));

        Assert.Equal(
            checkpointed.CurrentWorld.Id,
            restored.CurrentWorld.Id);
    }

    [Fact]
    public void Restore_RejectsCheckpointFromDifferentTimeline()
    {
        var first = CreateTimeline();
        var second = CreateTimeline();

        Assert.Throws<ArgumentException>(
            () => SimulationTimeline.Restore(
                first.Id,
                null,
                null,
                first.InitialCheckpoint,
                first.CurrentWorld,
                [first.InitialCheckpoint, second.InitialCheckpoint],
                []));
    }

    [Fact]
    public void Restore_RejectsDuplicateCheckpointIdentities()
    {
        var timeline = CreateTimeline();

        Assert.Throws<ArgumentException>(
            () => SimulationTimeline.Restore(
                timeline.Id,
                null,
                null,
                timeline.InitialCheckpoint,
                timeline.CurrentWorld,
                [
                    timeline.InitialCheckpoint,
                    timeline.InitialCheckpoint
                ],
                []));
    }

    [Fact]
    public void Restore_RejectsMismatchedCurrentWorld()
    {
        var timeline = CreateTimeline();

        Assert.Throws<ArgumentException>(
            () => SimulationTimeline.Restore(
                timeline.Id,
                null,
                null,
                timeline.InitialCheckpoint,
                new WorldState(
                    WorldId.New(),
                    SimulationTime.Zero),
                timeline.Checkpoints,
                []));
    }

    [Fact]
    public void Restore_RejectsHistoryBeyondCurrentTime()
    {
        var timeline = CreateTimeline();

        var futureCheckpoint =
            SimulationCheckpoint.Create(
                timeline.Id,
                timeline.CurrentWorld.AdvanceBy(60));

        Assert.Throws<ArgumentException>(
            () => SimulationTimeline.Restore(
                timeline.Id,
                null,
                null,
                timeline.InitialCheckpoint,
                timeline.CurrentWorld,
                [
                    timeline.InitialCheckpoint,
                    futureCheckpoint
                ],
                []));
    }

    [Fact]
    public void Restore_RejectsIncompleteParentIdentity()
    {
        var timeline = CreateTimeline();

        Assert.Throws<ArgumentException>(
            () => SimulationTimeline.Restore(
                timeline.Id,
                TimelineId.New(),
                null,
                timeline.InitialCheckpoint,
                timeline.CurrentWorld,
                timeline.Checkpoints,
                []));
    }

    private static SimulationTimeline CreateTimeline()
    {
        return SimulationTimeline.Create(
            new WorldState(
                WorldId.New(),
                SimulationTime.Zero));
    }
}
