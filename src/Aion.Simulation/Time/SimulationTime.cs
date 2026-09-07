namespace Aion.Simulation.Time;

public readonly record struct SimulationTime
{
    public SimulationTime(long totalSeconds)
    {
        if (totalSeconds < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(totalSeconds),
                "Simulation time cannot be negative.");
        }

        TotalSeconds = totalSeconds;
    }

    public long TotalSeconds { get; }

    public static SimulationTime Zero => new(0);

    public SimulationTime AdvanceBy(long seconds)
    {
        if (seconds < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(seconds),
                "Simulation time cannot advance by a negative duration.");
        }

        checked
        {
            return new SimulationTime(TotalSeconds + seconds);
        }
    }
}
