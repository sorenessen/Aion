namespace Est.Simulation.Worlds;

public readonly record struct WorldId
{
    public WorldId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "World identity cannot be empty.",
                nameof(value));
        }

        Value = value;
    }

    public Guid Value { get; }

    public static WorldId New()
    {
        return new WorldId(Guid.NewGuid());
    }
}
