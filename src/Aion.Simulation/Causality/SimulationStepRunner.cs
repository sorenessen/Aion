using Aion.Simulation.Operations;
using Aion.Simulation.Worlds;

namespace Aion.Simulation.Causality;

public static class SimulationStepRunner
{
    public static SimulationStepResult Step(
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

        var change = system.Evaluate(
            world,
            elapsedSeconds);

        if (change is null)
        {
            throw new InvalidOperationException(
                "A causal system must return a simulation change.");
        }

        var changedWorld =
            SimulationOperationExecutor.Apply(
                world,
                change.Operation);

        var advancedWorld =
            SimulationOperationExecutor.Apply(
                changedWorld,
                new AdvanceTimeOperation(elapsedSeconds));

        return new SimulationStepResult(
            advancedWorld,
            change);
    }
}
