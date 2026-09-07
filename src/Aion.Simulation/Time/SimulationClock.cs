namespace Aion.Simulation.Time;

public sealed class SimulationClock
{
    public SimulationClock(SimulationTime initialTime)
    {
        CurrentTime = initialTime;
    }

    public SimulationTime CurrentTime { get; private set; }

    public bool IsPaused { get; private set; }

    public void Pause()
    {
        IsPaused = true;
    }

    public void Resume()
    {
        IsPaused = false;
    }

    public void AdvanceBy(long seconds)
    {
        if (seconds < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(seconds),
                "Simulation time cannot advance by a negative duration.");
        }

        if (IsPaused)
        {
            return;
        }

        CurrentTime = CurrentTime.AdvanceBy(seconds);
    }
}
