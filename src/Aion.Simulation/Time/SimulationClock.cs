using Aion.Simulation.Worlds;

namespace Aion.Simulation.Time;

public sealed class SimulationClock
{
    public bool IsPaused { get; private set; }

    public void Pause()
    {
        IsPaused = true;
    }

    public void Resume()
    {
        IsPaused = false;
    }

    public WorldState Tick(WorldState world, long seconds)
    {
        ArgumentNullException.ThrowIfNull(world);

        if (seconds < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(seconds),
                "Simulation time cannot advance by a negative duration.");
        }

        if (IsPaused)
        {
            return world;
        }

        return world.AdvanceBy(seconds);
    }
}
