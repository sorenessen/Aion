using System.Collections.Immutable;
using Est.Simulation.Causality;
using Est.Simulation.Worlds;

namespace Est.Simulation.Timelines;

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

    public static SimulationTimeline Restore(
        TimelineId id,
        TimelineId? parentTimelineId,
        Guid? parentCheckpointId,
        SimulationCheckpoint initialCheckpoint,
        WorldState currentWorld,
        IEnumerable<SimulationCheckpoint> checkpoints,
        IEnumerable<TimelineEvent> events)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException(
                "Timeline identity cannot be empty.",
                nameof(id));
        }

        ArgumentNullException.ThrowIfNull(initialCheckpoint);
        ArgumentNullException.ThrowIfNull(currentWorld);
        ArgumentNullException.ThrowIfNull(checkpoints);
        ArgumentNullException.ThrowIfNull(events);

        if (parentTimelineId.HasValue != parentCheckpointId.HasValue)
        {
            throw new ArgumentException(
                "Parent timeline and checkpoint identities must be supplied together.");
        }

        if (parentTimelineId.HasValue &&
            parentTimelineId.Value.Value == Guid.Empty)
        {
            throw new ArgumentException(
                "Parent timeline identity cannot be empty.",
                nameof(parentTimelineId));
        }

        if (parentCheckpointId == Guid.Empty)
        {
            throw new ArgumentException(
                "Parent checkpoint identity cannot be empty.",
                nameof(parentCheckpointId));
        }

        if (parentTimelineId == id)
        {
            throw new ArgumentException(
                "A timeline cannot be its own parent.",
                nameof(parentTimelineId));
        }

        var checkpointArray = checkpoints.ToImmutableArray();
        var eventArray = events.ToImmutableArray();

        if (checkpointArray.IsEmpty ||
            checkpointArray.Any(checkpoint => checkpoint is null) ||
            eventArray.Any(timelineEvent => timelineEvent is null))
        {
            throw new ArgumentException(
                "Timeline history must contain an initial checkpoint and no null entries.");
        }

        if (initialCheckpoint.TimelineId != id ||
            checkpointArray.Any(checkpoint => checkpoint.TimelineId != id) ||
            eventArray.Any(timelineEvent => timelineEvent.TimelineId != id))
        {
            throw new ArgumentException(
                "Timeline history contains an inconsistent timeline identity.");
        }

        if (checkpointArray[0].Id != initialCheckpoint.Id)
        {
            throw new ArgumentException(
                "The first checkpoint must be the initial checkpoint.");
        }

        if (checkpointArray
            .Select(checkpoint => checkpoint.Id)
            .Distinct()
            .Count() != checkpointArray.Length ||
            eventArray
                .Select(timelineEvent => timelineEvent.Id)
                .Distinct()
                .Count() != eventArray.Length)
        {
            throw new ArgumentException(
                "Timeline history contains duplicate identities.");
        }

        var worldId = initialCheckpoint.World.Id;

        if (currentWorld.Id != worldId ||
            checkpointArray.Any(checkpoint => checkpoint.World.Id != worldId))
        {
            throw new ArgumentException(
                "Timeline checkpoints and current state must belong to the same world.");
        }

        var initialTime = initialCheckpoint.Time.TotalSeconds;
        var currentTime = currentWorld.CurrentTime.TotalSeconds;

        if (currentTime < initialTime ||
            checkpointArray.Any(checkpoint =>
                checkpoint.Time.TotalSeconds < initialTime ||
                checkpoint.Time.TotalSeconds > currentTime) ||
            eventArray.Any(timelineEvent =>
                timelineEvent.OccurredAt.TotalSeconds < initialTime ||
                timelineEvent.OccurredAt.TotalSeconds > currentTime))
        {
            throw new ArgumentException(
                "Timeline history must fall within its initial and current times.");
        }

        if (checkpointArray
            .Zip(checkpointArray.Skip(1))
            .Any(pair => pair.First.Time.TotalSeconds >
                         pair.Second.Time.TotalSeconds) ||
            eventArray
                .Zip(eventArray.Skip(1))
                .Any(pair => pair.First.OccurredAt.TotalSeconds >
                             pair.Second.OccurredAt.TotalSeconds))
        {
            throw new ArgumentException(
                "Timeline history must be ordered by simulation time.");
        }

        return new SimulationTimeline(
            id,
            parentTimelineId,
            parentCheckpointId,
            initialCheckpoint,
            currentWorld.Copy(),
            checkpointArray,
            eventArray);
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
                result.ElapsedSeconds);

        if (result.World.CurrentTime != expectedTime)
        {
            throw new InvalidOperationException(
                "The step result time does not match the timeline's current time and elapsed duration.");
        }

        var events = Events;

        foreach (var change in result.Changes)
        {
            if (change.ElapsedSeconds != result.ElapsedSeconds)
            {
                throw new InvalidOperationException(
                    "A recorded change must describe the step duration.");
            }

            events = events.Add(
                TimelineEvent.FromChange(
                    Id,
                    result.World.CurrentTime,
                    change));
        }

        return new SimulationTimeline(
            Id,
            ParentTimelineId,
            ParentCheckpointId,
            InitialCheckpoint,
            result.World.Copy(),
            Checkpoints,
            events);
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
