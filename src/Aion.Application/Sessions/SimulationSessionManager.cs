using System.Collections.Concurrent;
using Aion.Simulation.Definitions;
using Aion.Simulation.Timelines;
using Aion.Simulation.Worlds;

namespace Aion.Application.Sessions;

public sealed class SimulationSessionManager
{
    private readonly ConcurrentDictionary<
        SimulationSessionId,
        SimulationSession> _sessions = new();

    public SimulationSessionId Create(
        WorldState initialWorld)
    {
        return Create(initialWorld, SimulationDefinition.Empty);
    }

    public SimulationSessionId Create(
        WorldState initialWorld,
        SimulationDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(initialWorld);
        ArgumentNullException.ThrowIfNull(definition);

        var id = SimulationSessionId.New();
        var session = new SimulationSession(initialWorld, definition);

        if (!_sessions.TryAdd(id, session))
        {
            throw new InvalidOperationException(
                "Failed to create a unique simulation session.");
        }

        return id;
    }

    public SimulationSessionId Restore(
        SimulationTimeline timeline)
    {
        return Restore(timeline, SimulationDefinition.Empty);
    }

    public SimulationSessionId Restore(
        SimulationTimeline timeline,
        SimulationDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(timeline);
        ArgumentNullException.ThrowIfNull(definition);

        var id = SimulationSessionId.New();
        var session = new SimulationSession(timeline, definition);

        if (!_sessions.TryAdd(id, session))
        {
            throw new InvalidOperationException(
                "Failed to create a unique simulation session.");
        }

        return id;
    }

    public bool TryGet(
        SimulationSessionId id,
        out SimulationSession? session)
    {
        return _sessions.TryGetValue(id, out session);
    }

    public SimulationSession Get(
        SimulationSessionId id)
    {
        if (!_sessions.TryGetValue(id, out var session))
        {
            throw new KeyNotFoundException(
                $"Simulation session '{id.Value}' was not found.");
        }

        return session;
    }
}
