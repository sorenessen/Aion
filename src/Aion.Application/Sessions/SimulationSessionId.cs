namespace Aion.Application.Sessions;

public readonly record struct SimulationSessionId
{
    public SimulationSessionId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "Simulation session identity cannot be empty.",
                nameof(value));
        }

        Value = value;
    }

    public Guid Value { get; }

    public static SimulationSessionId New() =>
        new(Guid.NewGuid());
}
