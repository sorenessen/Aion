using Est.Simulation.Worlds;

namespace Est.Simulation.Operations;

public static class SimulationOperationExecutor
{
    public static WorldState Apply(
        WorldState world,
        ISimulationOperation operation)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(operation);

        return operation.Apply(world);
    }
}
