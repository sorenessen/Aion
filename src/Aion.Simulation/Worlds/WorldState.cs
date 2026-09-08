using Aion.Simulation.Time;

namespace Aion.Simulation.Worlds;

public sealed record WorldState
{
    public WorldState(WorldId id, SimulationTime currentTime)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException(
                "World identity cannot be empty.",
                nameof(id));
        }

        Id = id;
        CurrentTime = currentTime;
    }

    public WorldId Id { get; private init; }

    public SimulationTime CurrentTime { get; init; }

    public WorldState AdvanceBy(long seconds)
    {
        return this with
        {
            CurrentTime = CurrentTime.AdvanceBy(seconds)
        };
    }
}
