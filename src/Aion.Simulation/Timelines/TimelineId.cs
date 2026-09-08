namespace Aion.Simulation.Timelines;

public readonly record struct TimelineId
{
    public TimelineId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "Timeline identity cannot be empty.",
                nameof(value));
        }

        Value = value;
    }

    public Guid Value { get; }

    public static TimelineId New()
    {
        return new TimelineId(Guid.NewGuid());
    }
}
