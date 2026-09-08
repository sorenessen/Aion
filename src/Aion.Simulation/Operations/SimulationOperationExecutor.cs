using Aion.Simulation.Worlds;

namespace Aion.Simulation.Operations;

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
