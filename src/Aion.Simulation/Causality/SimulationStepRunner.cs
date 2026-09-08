using Aion.Simulation.Operations;
using Aion.Simulation.Worlds;

namespace Aion.Simulation.Causality;

public static class SimulationStepRunner
{
    public static WorldState Step(
        WorldState world,
        long elapsedSeconds,
        ICausalSystem system)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(system);

        if (elapsedSeconds < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(elapsedSeconds),
                "Simulation step duration cannot be negative.");
        }

        var operation = system.Evaluate(
            world,
            elapsedSeconds);

        if (operation is null)
        {
            throw new InvalidOperationException(
                "A causal system must return a simulation operation.");
        }

        var changedWorld =
            SimulationOperationExecutor.Apply(
                world,
                operation);

        return SimulationOperationExecutor.Apply(
            changedWorld,
            new AdvanceTimeOperation(elapsedSeconds));
    }
}
