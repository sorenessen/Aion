using System.Collections.Immutable;
using Aion.Simulation.Causality;
using Aion.Simulation.Worlds;

namespace Aion.Simulation.Timelines;

public sealed record SimulationTimeline
{
    public SimulationTimeline(
        TimelineId id,
        WorldState initialWorld)
        : this(
            id,
            initialWorld,
            null,
            null)
    {
    }

    private SimulationTimeline(
        TimelineId id,
        WorldState initialWorld,
        TimelineId? parentTimelineId,
        Guid? parentCheckpointId)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException(
                "Timeline identity cannot be empty.",
                nameof(id));
        }

        ArgumentNullException.ThrowIfNull(initialWorld);

        Id = id;
        ParentTimelineId = parentTimelineId;
        ParentCheckpointId = parentCheckpointId;

        InitialCheckpoint =
            SimulationCheckpoint.Create(
                id,
                initialWorld);

        CurrentWorld = initialWorld.Copy();

        Checkpoints =
            ImmutableArray.Create(
                InitialCheckpoint);

        Events =
            ImmutableArray<TimelineEvent>.Empty;
    }

    private SimulationTimeline(
        TimelineId id,
        TimelineId? parentTimelineId,
        Guid? parentCheckpointId,
        SimulationCheckpoint initialCheckpoint,
        WorldState currentWorld,
        ImmutableArray<SimulationCheckpoint> checkpoints,
        ImmutableArray<TimelineEvent> events)
    {
        Id = id;
        ParentTimelineId = parentTimelineId;
        ParentCheckpointId = parentCheckpointId;
        InitialCheckpoint = initialCheckpoint;
        CurrentWorld = currentWorld;
        Checkpoints = checkpoints;
        Events = events;
    }

    public TimelineId Id { get; }

    public TimelineId? ParentTimelineId { get; }

    public Guid? ParentCheckpointId { get; }

    public SimulationCheckpoint InitialCheckpoint { get; }

    public WorldState CurrentWorld { get; }

    public ImmutableArray<SimulationCheckpoint> Checkpoints { get; }

    public ImmutableArray<TimelineEvent> Events { get; }

    public static SimulationTimeline Create(
        WorldState initialWorld)
    {
        return new SimulationTimeline(
            TimelineId.New(),
            initialWorld);
    }

    public SimulationTimeline RecordStep(
        SimulationStepResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.World.Id != CurrentWorld.Id)
        {
            throw new InvalidOperationException(
                "A timeline cannot record a step from a different world.");
        }

        var expectedTime =
            CurrentWorld.CurrentTime.AdvanceBy(
                result.Change.ElapsedSeconds);

        if (result.World.CurrentTime != expectedTime)
        {
            throw new InvalidOperationException(
                "The step result time does not match the timeline's current time and elapsed duration.");
        }

        var timelineEvent =
            TimelineEvent.FromChange(
                Id,
                result.World.CurrentTime,
                result.Change);

        return new SimulationTimeline(
            Id,
            ParentTimelineId,
            ParentCheckpointId,
            InitialCheckpoint,
            result.World.Copy(),
            Checkpoints,
            Events.Add(timelineEvent));
    }

    public SimulationTimeline CreateCheckpoint()
    {
        var checkpoint =
            SimulationCheckpoint.Create(
                Id,
                CurrentWorld);

        return new SimulationTimeline(
            Id,
            ParentTimelineId,
            ParentCheckpointId,
            InitialCheckpoint,
            CurrentWorld.Copy(),
            Checkpoints.Add(checkpoint),
            Events);
    }

    public SimulationTimeline ForkFromCheckpoint(
        Guid checkpointId)
    {
        if (checkpointId == Guid.Empty)
        {
            throw new ArgumentException(
                "Checkpoint identity cannot be empty.",
                nameof(checkpointId));
        }

        var checkpoint = Checkpoints
            .FirstOrDefault(
                candidate =>
                    candidate.Id == checkpointId);

        if (checkpoint is null)
        {
            throw new InvalidOperationException(
                "The checkpoint does not belong to this timeline.");
        }

        var childWorld =
            checkpoint.World.Fork();

        return new SimulationTimeline(
            TimelineId.New(),
            childWorld,
            Id,
            checkpoint.Id);
    }

    public SimulationTimeline ResetToInitial()
    {
        return ForkFromCheckpoint(
            InitialCheckpoint.Id);
    }
}
