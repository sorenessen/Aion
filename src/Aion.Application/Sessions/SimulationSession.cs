using Aion.Simulation.Causality;
using Aion.Simulation.Operations;
using Aion.Simulation.Planets;
using Aion.Simulation.Time;
using Aion.Simulation.Timelines;
using Aion.Simulation.Worlds;

namespace Aion.Application.Sessions;

public sealed class SimulationSession
{
    private readonly object _sync = new();
    private readonly SimulationClock _clock = new();
    private SimulationTimeline _timeline;

    public SimulationSession(WorldState initialWorld)
        : this(SimulationTimeline.Create(initialWorld))
    {
    }

    public SimulationSession(SimulationTimeline timeline)
    {
        ArgumentNullException.ThrowIfNull(timeline);
        _timeline = timeline;
    }

    public SimulationTimeline Timeline
    {
        get
        {
            lock (_sync)
                return _timeline;
        }
    }

    public WorldState CurrentWorld => Timeline.CurrentWorld;

    public bool IsPaused
    {
        get
        {
            lock (_sync)
                return _clock.IsPaused;
        }
    }

    public void Pause()
    {
        lock (_sync)
            _clock.Pause();
    }

    public void Resume()
    {
        lock (_sync)
            _clock.Resume();
    }

    public SimulationTimeline Advance(long seconds)
    {
        if (seconds < 0)
            throw new ArgumentOutOfRangeException(nameof(seconds));

        lock (_sync)
        {
            return AdvanceCore(seconds);
        }
    }

    public SimulationTimeline Tick(long seconds)
    {
        if (seconds < 0)
            throw new ArgumentOutOfRangeException(nameof(seconds));

        lock (_sync)
        {
            if (_clock.IsPaused)
                return _timeline;

            return AdvanceCore(seconds);
        }
    }

    public SimulationTimeline ReplacePlanetEnvironment(
        PlanetId planetId,
        PlanetEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(environment);

        lock (_sync)
        {
            var operation =
                new ReplacePlanetEnvironmentOperation(
                    planetId,
                    environment);

            var world =
                SimulationOperationExecutor.Apply(
                    _timeline.CurrentWorld,
                    operation);

            var change =
                new SimulationChange(
                    operation,
                    "User intervention",
                    "Replaced planetary environment.",
                    planetId,
                    0);

            _timeline =
                _timeline.RecordStep(
                    new SimulationStepResult(
                        world,
                        change));

            return _timeline;
        }
    }

    private SimulationTimeline AdvanceCore(
        long seconds)
    {
        var operation =
            new AdvanceTimeOperation(seconds);

        var world =
            SimulationOperationExecutor.Apply(
                _timeline.CurrentWorld,
                operation);

        var change =
            new SimulationChange(
                operation,
                "Explicit time advancement",
                $"Advanced simulation time by {seconds} seconds.",
                null,
                seconds);

        _timeline =
            _timeline.RecordStep(
                new SimulationStepResult(
                    world,
                    change));

        return _timeline;
    }
}
