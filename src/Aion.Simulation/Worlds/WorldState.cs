using Aion.Simulation.Time;

namespace Aion.Simulation.Worlds;

public sealed record WorldState
{
    public WorldState(WorldId id, SimulationTime currentTime)
    {
        Id = id;
        CurrentTime = currentTime;
    }

    public WorldId Id { get; init; }

    public SimulationTime CurrentTime { get; init; }

    public WorldState AdvanceBy(long seconds)
    {
        return this with
        {
            CurrentTime = CurrentTime.AdvanceBy(seconds)
        };
    }
}
