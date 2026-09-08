using Est.Simulation.Worlds;

namespace Est.Simulation.Operations;

public sealed record AdvanceTimeOperation : ISimulationOperation
{
    public AdvanceTimeOperation(long seconds)
    {
        if (seconds < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(seconds),
                "Simulation time cannot advance by a negative duration.");
        }

        Seconds = seconds;
    }

    public long Seconds { get; }

    public WorldState Apply(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);

        return world.AdvanceBy(Seconds);
    }
}
