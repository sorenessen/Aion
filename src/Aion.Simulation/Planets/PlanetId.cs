namespace Aion.Simulation.Planets;

public readonly record struct PlanetId
{
    public PlanetId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "Planet identity cannot be empty.",
                nameof(value));
        }

        Value = value;
    }

    public Guid Value { get; }

    public static PlanetId New()
    {
        return new PlanetId(Guid.NewGuid());
    }
}
