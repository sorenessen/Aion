using Aion.Simulation.Time;
using Aion.Simulation.Timelines;
using Aion.Simulation.Worlds;

namespace Aion.Simulation.Tests.Timelines;

public class SimulationTimelineTests
{
    [Fact]
    public void Create_EstablishesInitialCheckpoint()
    {
        var world = new WorldState(
            WorldId.New(),
            new SimulationTime(100));

        var timeline = SimulationTimeline.Create(world);

        Assert.NotEqual(Guid.Empty, timeline.Id.Value);
        Assert.Equal(
            timeline.Id,
            timeline.InitialCheckpoint.TimelineId);

        Assert.Equal(
            world.Id,
            timeline.CurrentWorld.Id);

        Assert.Equal(
            100,
            timeline.CurrentWorld.CurrentTime.TotalSeconds);

        Assert.Single(timeline.Checkpoints);
        Assert.Same(
            timeline.InitialCheckpoint,
            timeline.Checkpoints[0]);
    }

    [Fact]
    public void Create_DoesNotShareMutableWorldContainer()
    {
        var world = new WorldState(
            WorldId.New(),
            SimulationTime.Zero);

        var timeline = SimulationTimeline.Create(world);

        Assert.NotSame(world, timeline.CurrentWorld);
        Assert.NotSame(
            world,
            timeline.InitialCheckpoint.World);
    }
    [Fact]
    public void RecordStep_AppendsEventAndUpdatesCurrentWorld()
    {
        var world = new WorldState(
            WorldId.New(),
            new SimulationTime(100));

        var timeline =
            SimulationTimeline.Create(world);

        var result = CreateStepResult(
            world,
            60);

        var updated =
            timeline.RecordStep(result);

        Assert.Equal(
            160,
            updated.CurrentWorld.CurrentTime.TotalSeconds);

        var timelineEvent =
            Assert.Single(updated.Events);

        Assert.Equal(
            updated.Id,
            timelineEvent.TimelineId);

        Assert.Equal(
            160,
            timelineEvent.OccurredAt.TotalSeconds);

        Assert.Equal(
            "test-step",
            timelineEvent.Cause);
    }

    [Fact]
    public void RecordStep_DoesNotModifySourceTimeline()
    {
        var world = new WorldState(
            WorldId.New(),
            new SimulationTime(100));

        var timeline =
            SimulationTimeline.Create(world);

        var updated =
            timeline.RecordStep(
                CreateStepResult(
                    world,
                    60));

        Assert.Empty(timeline.Events);

        Assert.Equal(
            100,
            timeline.CurrentWorld.CurrentTime.TotalSeconds);

        Assert.Single(updated.Events);

        Assert.Equal(
            160,
            updated.CurrentWorld.CurrentTime.TotalSeconds);
    }

    [Fact]
    public void RecordStep_RejectsDifferentWorld()
    {
        var timeline =
            SimulationTimeline.Create(
                new WorldState(
                    WorldId.New(),
                    SimulationTime.Zero));

        var otherWorld =
            new WorldState(
                WorldId.New(),
                new SimulationTime(60));

        var result =
            new Aion.Simulation.Causality.SimulationStepResult(
                otherWorld,
                new Aion.Simulation.Causality.SimulationChange(
                    new Aion.Simulation.Operations.AdvanceTimeOperation(0),
                    "test-step",
                    "Test step.",
                    null,
                    60));

        Assert.Throws<InvalidOperationException>(
            () => timeline.RecordStep(result));
    }

    private static Aion.Simulation.Causality.SimulationStepResult
        CreateStepResult(
            WorldState world,
            long elapsedSeconds)
    {
        return new Aion.Simulation.Causality.SimulationStepResult(
            world.AdvanceBy(elapsedSeconds),
            new Aion.Simulation.Causality.SimulationChange(
                new Aion.Simulation.Operations.AdvanceTimeOperation(0),
                "test-step",
                "Test step.",
                null,
                elapsedSeconds));
    }

    [Fact]
    public void CreateCheckpoint_AppendsCheckpointWithoutChangingHistory()
    {
        var world = new WorldState(
            WorldId.New(),
            new SimulationTime(100));

        var timeline =
            SimulationTimeline.Create(world);

        var updated =
            timeline.CreateCheckpoint();

        Assert.Single(timeline.Checkpoints);
        Assert.Equal(2, updated.Checkpoints.Length);

        Assert.Equal(
            timeline.Id,
            updated.Id);

        Assert.Equal(
            100,
            updated.Checkpoints[1]
                .Time
                .TotalSeconds);
    }

    [Fact]
    public void ForkFromCheckpoint_CreatesChildTimeline()
    {
        var world = new WorldState(
            WorldId.New(),
            new SimulationTime(100));

        var parent =
            SimulationTimeline.Create(world);

        var child =
            parent.ForkFromCheckpoint(
                parent.InitialCheckpoint.Id);

        Assert.NotEqual(
            parent.Id,
            child.Id);

        Assert.Equal(
            parent.Id,
            child.ParentTimelineId);

        Assert.Equal(
            parent.InitialCheckpoint.Id,
            child.ParentCheckpointId);
    }

    [Fact]
    public void ForkFromCheckpoint_CreatesDistinctWorldIdentity()
    {
        var world = new WorldState(
            WorldId.New(),
            new SimulationTime(100));

        var parent =
            SimulationTimeline.Create(world);

        var child =
            parent.ForkFromCheckpoint(
                parent.InitialCheckpoint.Id);

        Assert.NotEqual(
            parent.CurrentWorld.Id,
            child.CurrentWorld.Id);

        Assert.Equal(
            parent.InitialCheckpoint.World.CurrentTime,
            child.CurrentWorld.CurrentTime);
    }

    [Fact]
    public void ForkFromCheckpoint_DoesNotModifyParentTimeline()
    {
        var world = new WorldState(
            WorldId.New(),
            new SimulationTime(100));

        var parent =
            SimulationTimeline.Create(world);

        var child =
            parent.ForkFromCheckpoint(
                parent.InitialCheckpoint.Id);

        Assert.Null(parent.ParentTimelineId);
        Assert.Null(parent.ParentCheckpointId);
        Assert.Empty(parent.Events);
        Assert.Single(parent.Checkpoints);

        Assert.NotEqual(
            parent.Id,
            child.Id);
    }

    [Fact]
    public void ForkFromCheckpoint_RejectsUnknownCheckpoint()
    {
        var timeline =
            SimulationTimeline.Create(
                new WorldState(
                    WorldId.New(),
                    SimulationTime.Zero));

        Assert.Throws<InvalidOperationException>(
            () => timeline.ForkFromCheckpoint(
                Guid.NewGuid()));
    }

    [Fact]
    public void ResetToInitial_CreatesChildAtInitialState()
    {
        var original =
            new WorldState(
                WorldId.New(),
                new SimulationTime(100));

        var timeline =
            SimulationTimeline.Create(original);

        var advanced =
            timeline.RecordStep(
                CreateStepResult(
                    original,
                    60));

        var reset =
            advanced.ResetToInitial();

        Assert.Equal(
            100,
            reset.CurrentWorld
                .CurrentTime
                .TotalSeconds);

        Assert.Equal(
            advanced.Id,
            reset.ParentTimelineId);

        Assert.Equal(
            advanced.InitialCheckpoint.Id,
            reset.ParentCheckpointId);

        Assert.NotEqual(
            advanced.CurrentWorld.Id,
            reset.CurrentWorld.Id);
    }

    [Fact]
    public void ForkFromLaterCheckpoint_StartsAtCheckpointState()
    {
        var original =
            new WorldState(
                WorldId.New(),
                new SimulationTime(100));

        var timeline =
            SimulationTimeline.Create(original);

        var advanced =
            timeline.RecordStep(
                CreateStepResult(
                    original,
                    60));

        var checkpointed =
            advanced.CreateCheckpoint();

        var laterCheckpoint =
            checkpointed.Checkpoints[1];

        var child =
            checkpointed.ForkFromCheckpoint(
                laterCheckpoint.Id);

        Assert.Equal(
            160,
            child.CurrentWorld
                .CurrentTime
                .TotalSeconds);

        Assert.Equal(
            laterCheckpoint.Id,
            child.ParentCheckpointId);
    }

    [Fact]
    public void ForkFromEarlierCheckpoint_DoesNotInheritParentFuture()
    {
        var original =
            new WorldState(
                WorldId.New(),
                new SimulationTime(100));

        var timeline =
            SimulationTimeline.Create(original);

        var advanced =
            timeline.RecordStep(
                CreateStepResult(
                    original,
                    60));

        var checkpointed =
            advanced.CreateCheckpoint();

        var advancedAgain =
            checkpointed.RecordStep(
                CreateStepResult(
                    checkpointed.CurrentWorld,
                    60));

        var child =
            advancedAgain.ForkFromCheckpoint(
                advancedAgain.InitialCheckpoint.Id);

        Assert.Equal(
            100,
            child.CurrentWorld
                .CurrentTime
                .TotalSeconds);

        Assert.Empty(child.Events);
        Assert.Single(child.Checkpoints);

        Assert.Equal(
            2,
            advancedAgain.Events.Length);

        Assert.Equal(
            2,
            advancedAgain.Checkpoints.Length);
    }

}
